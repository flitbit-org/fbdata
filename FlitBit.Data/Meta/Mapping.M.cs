#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Emit;
using FlitBit.ObjectIdentity;
using FlitBit.Wireup;
using FlitBit.Wireup.Recording;
using Inflector;

namespace FlitBit.Data.Meta
{
	
	/// <summary>
	///   Default mapping implementation for type TModel.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	internal class Mapping<TModel> : IMapping<TModel>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _revision;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly HierarchyMapping<TModel> _hierarchyMapping = new HierarchyMapping<TModel>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly IdentityKey<TModel> _identityKey;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly List<IMapping> _baseTypes = new List<IMapping>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Dictionary<string, CollectionMapping<TModel>> _collections =
			new Dictionary<string, CollectionMapping<TModel>>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly List<ColumnMapping> _columns = new List<ColumnMapping>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly List<ColumnMapping> _declaredColumns = new List<ColumnMapping>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly List<Dependency> _dependencies = new List<Dependency>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly IdentityMapping<TModel> _identity;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly NaturalKeyMapping<TModel> _naturalKey;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Object _sync = new Object();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ConcurrentQueue<Action<IMapping>> _whenCompleted = new ConcurrentQueue<Action<IMapping>>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IDataModelBinder _binder;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string _connectionName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		List<string> _errors;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		DbProviderHelper _helper;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string _targetSchema;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string _targetObject;

		internal Mapping()
		{
			_hierarchyMapping = new HierarchyMapping<TModel>();
			_hierarchyMapping.OnChanged += (sender, e) => Interlocked.Increment(ref _revision);
			_identityKey = FactoryProvider.Factory.CreateInstance<IdentityKey<TModel>>();
			_identity = new IdentityMapping<TModel>(this);
			_naturalKey = new NaturalKeyMapping<TModel>(this);			
		}

		public HierarchyMapping<TModel> Hierarchy { get { return _hierarchyMapping; } } 
		public IdentityKey<TModel> IdentityKey { get { return _identityKey; } }

		/// <summary>
		///   Indicates the entity's behaviors.
		/// </summary>
		public EntityBehaviors Behaviors { get; internal set; }
		
		public IEnumerable<string> Errors { get { return (_errors == null) ? new String[0] : _errors.ToArray(); } }

		public bool HasBinder
		{
			get
			{
				return (!String.IsNullOrEmpty(ConnectionName))
					&& GetDbProviderHelper() != null;
			}
		}

		public bool HasIdentity { get { return this._identity.Columns.Any(); } }

		/// <summary>
		///   Gets the mapped type's identity mapping.
		/// </summary>
		public IdentityMapping Identity { get { return _identity; } }

		public NaturalKeyMapping<TModel> NaturalKey { get { return _naturalKey; } }

		public CollectionMapping<TModel> Collection(Expression<Func<TModel, object>> expression)
		{
			Contract.Requires<ArgumentNullException>(expression != null);

			var member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member");

			var name = member.Name;

			CollectionMapping<TModel> col;
			lock (_sync)
			{
				if (!_collections.TryGetValue(name, out col))
				{
					_collections.Add(name, col = new CollectionMapping<TModel>(this, member, name));
				}
			}
			return col;
		}

		/// <summary>
		///   Maps a column to a member (property or field) of the object.
		///   The property or field indicated will serve as the column definition.
		/// </summary>
		/// <param name="expression">
		///   An expression that identifies the member
		///   upon which a column will be mapped.
		/// </param>
		/// <returns>
		///   A ColumnMapping object for further refinement of the column's
		///   definition.
		/// </returns>
		public ColumnMapping<TModel> Column(Expression<Func<TModel, object>> expression)
		{
			Contract.Requires<ArgumentNullException>(expression != null);

			var member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member");

			return DefineColumn(member);
		}

		/// <summary>
		///   Completes the mapping.
		/// </summary>
		/// <returns></returns>
		public Mappings End()
		{
			if (!HasIdentity)
			{
				// Try to discover identity columns from column definitions.
				foreach (var c in Columns.Where(c => c.Behaviors.HasFlag(ColumnBehaviors.Identity)))
				{
					Identity.AddColumn(c, SortOrderKind.Asc);
				}
			}
			this.MarkComplete();
			return Mappings.Instance;
		}

		/// <summary>
		///   Gets the DbProviderHelper associated with the mapping's connection.
		/// </summary>
		/// <returns></returns>
		public DbProviderHelper GetDbProviderHelper()
		{
			if (_helper != null) return _helper;
			if (String.IsNullOrEmpty(ConnectionName))
			{
				throw new InvalidOperationException(String.Concat("ConnectionName missing on type: ",
					RuntimeType.GetReadableSimpleName(), "."));
			}
			_helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(ConnectionName);
			return _helper;
		}

		public IMapping<TModel> InSchema(string schema)
		{
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);

			this.TargetSchema = schema;
			return this;
		}

		public MemberInfo InferCollectionReferenceTargetMember(MemberInfo member, IMapping elmMapping)
		{
			var foreignObjectID = elmMapping.GetPreferredReferenceColumn();
			if (foreignObjectID == null)
			{
				throw new MappingException(String.Concat("Relationship not defined between ", typeof(TModel).Name, ".", member.Name,
																								" and the referenced type: ", elmMapping.RuntimeType.Name));
			}

			AddDependency(elmMapping, DependencyKind.Soft, member);

			return foreignObjectID.Member;
		}

		public IMapping<TModel> MapAllOperations()
		{
			return MapAllOperations(this.Strategy);
		}

		public IMapping<TModel> MapAllOperations(MappingStrategy strategy)
		{
			this.Strategy = strategy;
			var errors = new List<string>();
			var warnings = new List<string>();

			if (String.IsNullOrEmpty(this.ConnectionName))
			{
				errors.Add("ConnectionName has not been configured.");
			}

			if (String.IsNullOrEmpty(this.TargetCatalog))
			{
				warnings.Add("TargetCatalog has not been configured; the catalog will be determined by the connection string.");
			}
			if (String.IsNullOrEmpty(this.TargetSchema))
			{
				warnings.Add("TargetSchema has not been configured; none will be used.");
			}
			if (String.IsNullOrEmpty(this.TargetObject))
			{
				warnings.Add(String.Concat("TargetObject has not been configured; the type name will be used: ", typeof(TModel).Name));
			}

			//using (var container = Create.NewContainer())
			//{
			//  var cn = container.Scope.Add(DbExtensions.CreateAndOpenConnection(this.ConnectionName));
			//  var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
			//  if (String.IsNullOrEmpty(this.TargetCatalog)) this.TargetCatalog = cn.Database;
			//  else if (!String.Equals(cn.Database, this.TargetCatalog))
			//  {
			//    cn.ChangeDatabase(this.TargetCatalog);
			//  }
			//  if (!helper.SchemaExists(cn, this.TargetCatalog, this.TargetSchema))
			//  {
			//    helper.CreateSchema(cn, this.TargetCatalog, this.TargetSchema);
			//  }
			//}
			_errors = errors;

			return this;
		}

		public IMapping<TModel> ReferencesType<TOther>(Expression<Func<TModel, TOther, bool>> expression)
		{
			return this;
		}

		public IMapping<TModel> UsesConnection(string connection)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);

			this.ConnectionName = connection;
			return this;
		}

		/// <summary>
		///   Sets the database object name
		/// </summary>
		/// <param name="name">
		///   Name of the database object where data is stored for
		///   the type.
		/// </param>
		/// <returns></returns>
		public IMapping<TModel> WithName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);

			this.TargetObject = name;
			return this;
		}

		public MappedDbTypeEmitter GetColumnEmitter(ColumnMapping column)
		{
			var helper = this.GetDbProviderHelper();
			return (helper != null) ? helper.GetDbTypeEmitter(this, column) : null;
		}

		public void AddContributedColumn(ColumnMapping contributed)
		{
			var name = contributed.TargetName;
			lock (_sync)
			{
				if (this._columns.SingleOrDefault(c => c.TargetName == name) != null)
				{
					throw new MappingException(String.Concat("Duplicate column definition: ", name));
				}

				_declaredColumns.Add(contributed);
				_columns.Add(contributed);
			}
			Interlocked.Increment(ref _revision);
		}

		public void AddDependency(IMapping target, DependencyKind kind, MemberInfo member)
		{
			lock (_sync)
			{
				var dep = _dependencies.Find(d => d.Target == target);
				if (dep == null)
				{
					_dependencies.Add(new Dependency(kind, this, member, target).CalculateDependencyKind());
					Interlocked.Increment(ref _revision);
				}
			}
		}

		public CollectionMapping<TModel> DefineCollection(MemberInfo member, string name)
		{
      Contract.Requires<ArgumentNullException>(member != null);
      Contract.Requires<ArgumentNullException>(name != null);
		  if (member.MemberType != MemberTypes.Property
		      && typeof(TModel).GetProperty(name) != null)
		  {
        throw new MappingException(String.Concat("Collection name defined on method ", member.Name, " conflicts with an existing property's name: ", name, "."));
		  }

			CollectionMapping<TModel> col;
			lock (_sync)
			{
				if (!_collections.TryGetValue(name, out col))
				{
					_collections.Add(name, col = new CollectionMapping<TModel>(this, member, name));
				}
			}
			return col;
		}

		public ColumnMapping<TModel> DefineColumn(MemberInfo member)
		{
			var name = member.Name;

			ColumnMapping col;
			lock (_sync)
			{
				if (this._columns.SingleOrDefault(c => c.TargetName == name) != null)
				{
					throw new MappingException(String.Concat("Duplicate column definition: ", name));
				}

				col = ColumnMapping.FromMember<TModel>(this, member, _columns.Count);
				_declaredColumns.Add(col);
				_columns.Add(col);
			}
			return (ColumnMapping<TModel>) col;
		}

		public IMapping<TModel> InitFromMetadata()
		{
			var typ = typeof(TModel);
			var mod = typ.Module;
			var asm = typ.Assembly;
			using (var ctx = new WireupContext())
			{
				WireupCoordinator.Instance.WireupDependencies(ctx, asm);
			}
			// module, then assembly - module takes precedence
			if (mod.IsDefined(typeof(MapConnectionAttribute), false))
			{
				var conn = (MapConnectionAttribute) mod
					.GetCustomAttributes(typeof(MapConnectionAttribute), false)
					.First();
				conn.PrepareMapping(this);
			}
			else if (asm.IsDefined(typeof(MapConnectionAttribute), false))
			{
				var conn = (MapConnectionAttribute) asm
					.GetCustomAttributes(typeof(MapConnectionAttribute), false)
					.First();
				conn.PrepareMapping(this);
			}
			if (mod.IsDefined(typeof(MapSchemaAttribute), false))
			{
				var schema = (MapSchemaAttribute) mod
					.GetCustomAttributes(typeof(MapSchemaAttribute), false)
					.First();
				schema.PrepareMapping(this);
			}
			else if (asm.IsDefined(typeof(MapSchemaAttribute), false))
			{
				var schema = (MapSchemaAttribute) asm
					.GetCustomAttributes(typeof(MapSchemaAttribute), false)
					.First();
				schema.PrepareMapping(this);
			}
			foreach (var it in typeof(TModel).GetTypeHierarchyInDeclarationOrder()
																			.Except(new[]
																			{
																				typeof(Object),
																				typeof(INotifyPropertyChanged)
																			}))
			{
				if (it.IsDefined(typeof(MapEntityAttribute), false))
				{
					if (it != typeof(TModel))
					{
						var baseMapping = Mappings.AccessMappingFor(it);
						baseMapping.NotifySubtype(this);
						_baseTypes.Add(baseMapping);
						this.AddDependency(baseMapping, DependencyKind.Base, null);
						_columns.AddRange(baseMapping.DeclaredColumns);
						foreach (var idcol in baseMapping.Identity.Columns.Where(c => c.Column.Member.DeclaringType == baseMapping.RuntimeType))
						{
							Identity.AddColumn(idcol.Column, SortOrderKind.Asc);
						}
					}
					else
					{
						var entity = (MapEntityAttribute) it
							.GetCustomAttributes(typeof(MapEntityAttribute), false)
							.Single();
						entity.PrepareMapping(this, it);
					}
				}
			}
			if (Behaviors.HasFlag(EntityBehaviors.MapEnum))
			{
				var idcol = this.Identity.Columns[1].Column;
				if (!idcol.RuntimeType.IsEnum)
				{
					throw new MappingException(String.Concat("Entity type ", typeof(TModel).Name,
																									" declares behavior EntityBehaviors.MapEnum but the enum type cannot be determined. Specify an identity column of enum type."));
				}
				var namecol = Columns.FirstOrDefault(c => c.RuntimeType == typeof(String) && c.IsAlternateKey);
				if (namecol == null)
				{
					throw new MappingException(String.Concat("Entity type ", typeof(TModel).Name,
																									" declares behavior EntityBehaviors.MapEnum but a column to hold the enum name cannot be determined. Specify a string column with alternate key behavior."));
				}
				var names = Enum.GetNames(idcol.Member.GetTypeOfValue());
				namecol.VariableLength = names.Max(n => n.Length);
			}
			Interlocked.Increment(ref _revision);
			return this;
		}

		public void MapCollectionFromMeta(MemberInfo member, MapCollectionAttribute attr)
		{
      Contract.Requires<ArgumentException>(member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Method);
		  var type = (member.MemberType == MemberTypes.Property) ? ((PropertyInfo)member).PropertyType : ((MethodInfo)member).ReturnType;
			var elmType = type.FindElementType();
			IMapping elmMapping;

			if (elmType == RuntimeType)
			{
				elmMapping = this;
			} 
			else if (Mappings.ExistsFor(elmType))
			{
				elmMapping = Mappings.AccessMappingFor(elmType);
			}
			else
			{
				throw new MappingException(String.Concat("Unable to fulfill collection mapping on ", typeof (TModel).Name, ".", member.Name,
						" because the property must reference a mapped type.")
						);
			}
			var localProps = attr.LocalProperties.ToArray();
			if (localProps.Length == 0)
			{
				localProps = Identity.Columns.Select(c => c.Column.Member.Name).ToArray();
			}
			var referencedProps = attr.ReferencedProperties.ToArray();
			if (referencedProps.Length != localProps.Length)
			{
				throw new MappingException(String.Concat("The mapped collection on ", typeof (TModel).Name, ".", member.Name,
					" must identify the same number of join properties on both sides of the reference.")
					);
			}
			var locals = new List<MemberInfo>();
			foreach (var name in localProps)
			{
				var pp = typeof (TModel).GetProperty(name);
				if (pp == null)
				{
					throw new MappingException(String.Concat("The mapped collection on ", typeof(TModel).Name, ".", member.Name,
						" names a local property that does not exist: ", name, ".")
						);
				}
				locals.Add(pp);
			}

      var joinType = attr.JoinType;
      var joins = new List<MemberInfo>();
		  IMapping joinMapping = null;
      
			var referenced = new List<MemberInfo>();
		  if (joinType == null)
		  {
		    foreach (var name in referencedProps)
		    {
		      var pp = elmType.GetProperty(name);
		      if (pp == null)
		      {
		        throw new MappingException(String.Concat("The mapped collection on ", typeof(TModel).Name, ".", member.Name,
		          " references a property that does not exist: ", elmType.Name, ".", name, ".")
		          );
		      }
		      referenced.Add(pp);
		    }
        AddDependency(elmMapping, DependencyKind.Soft, member);
		  }
		  else
		  {
        if (joinType == RuntimeType)
        {
          joinMapping = this;
        }
        else if (Mappings.ExistsFor(joinType))
        {
          joinMapping = Mappings.AccessMappingFor(joinType);
        }
        else
        {
          throw new MappingException(String.Concat("Unable to fulfill collection mapping on ", typeof(TModel).Name, ".", member.Name,
              " because the property must only reference mapped type.")
              );
        }
        foreach (var name in referencedProps)
        {
          var pp = joinType.GetProperty(name);
          if (pp == null)
          {
            throw new MappingException(String.Concat("The mapped collection on ", typeof(TModel).Name, ".", member.Name,
              " references a property that does not exist: ", joinType.Name, ".", name, ".")
              );
          }
          referenced.Add(pp);
        }
        foreach (var name in attr.JoinProperties)
        {
          var pp = joinType.GetProperty(name);
          if (pp == null)
          {
            throw new MappingException(String.Concat("The mapped collection on ", typeof(TModel).Name, ".", member.Name,
              " references a property that does not exist: ", joinType.Name, ".", name, ".")
              );
          }
          joins.Add(pp);
        }
        AddDependency(joinMapping, DependencyKind.Soft, member);
        AddDependency(elmMapping, DependencyKind.Soft, member);
		  }
			var coll = DefineCollection(member, attr.Name ?? member.Name);
		  coll.ReferenceBehaviors = attr.Behaviors;
			coll.ReferencedType = elmType;
			coll.ReferencedProperties = referenced;
			coll.ReferencedMapping = elmMapping;
			coll.LocalProperties = locals;
		  coll.JoinType = attr.JoinType;
		  coll.JoinProperties = joins;
		  coll.JoinMapping = joinMapping;
		}

		public void MapColumnFromMeta(PropertyInfo p, MapColumnAttribute mapColumn)
		{
			var column = this.DefineColumn(p);
			if (!String.IsNullOrEmpty(mapColumn.TargetName))
			{
				column.WithTargetName(mapColumn.TargetName);
			}
			var behaviors = mapColumn.Behaviors;
			if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				behaviors |= ColumnBehaviors.Nullable;
			}
			column.WithBehaviors(behaviors);
			if (mapColumn.Length > 0)
			{
				column.WithVariableLength(mapColumn.Length);
			}
			if (column.Behaviors.HasFlag(ColumnBehaviors.Identity))
			{
				Identity.AddColumn(column, SortOrderKind.Asc);
			}

			if (Mappings.ExistsFor(p.PropertyType))
			{
				var foreignMapping = Mappings.AccessMappingFor(p.PropertyType);
				ColumnMapping foreignColumn;
				AddDependency(foreignMapping, DependencyKind.Direct, column.Member);

				if (mapColumn.References == null || !mapColumn.References.Any())
				{
					foreignColumn = foreignMapping.GetPreferredReferenceColumn();
					if (foreignColumn == null)
					{
						throw new MappingException(String.Concat("Relationship not defined between ", typeof(TModel).Name, ".", p.Name,
																										" and the referenced type: ", p.PropertyType.Name));
					}

					mapColumn.References = new[] {foreignColumn.Member.Name};
				}
				else
				{
					// Only 1 reference column for now.
					foreignColumn = foreignMapping.Columns.FirstOrDefault(c => c.Member.Name == mapColumn.References.First());
				}

				if (foreignColumn == null)
				{
					throw new InvalidOperationException(String.Concat("Property '", p.Name,
																														"' references an entity but a relationship cannot be determined."));
				}
				column.DefineReference(foreignColumn, mapColumn.ReferenceBehaviors);
			}
		}

		public void MarkComplete()
		{
			this.IsComplete = true;
			Action<IMapping> a;
			while (_whenCompleted.TryDequeue(out a))
			{
				a(this);
			}
		}

		#region IMapping<TModel> Members

		/// <summary>
		///   Indicates the data model's runtime type.
		/// </summary>
		public Type RuntimeType { get { return typeof(TModel); } }

		/// <summary>
		///   Indicates whether the mapping has been completed.
		/// </summary>
		public bool IsComplete { get; private set; }

		/// <summary>
		///   Indicates whether the entity is an enum type.
		/// </summary>
		public bool IsEnum { get { return Behaviors.HasFlag(EntityBehaviors.MapEnum); } }

		/// <summary>
		///   Indicates whether the entity's database object name is pluralized.
		/// </summary>
		public bool IsPluralized { get { return Behaviors.HasFlag(EntityBehaviors.Pluralized); } }

	  /// <summary>
	  /// Indicates whether the mapping behaves like a lookup list.
	  /// </summary>
	  public bool IsLookupList { get { return Behaviors.HasFlag(EntityBehaviors.LookupList); } }
    
		/// <summary>
		///   The Db object to which type T maps; either a table or view.
		/// </summary>
		public string TargetObject
		{
			get { return _targetObject; }
			set
			{
				_targetObject = (IsPluralized) ? value.Pluralize() : value;
			}
		}

		/// <summary>
		///   The Db schema where the target object resides.
		/// </summary>
		public string TargetSchema { get { return String.IsNullOrEmpty(_targetSchema) ? Mappings.Instance.DefaultSchema : _targetSchema; } set { _targetSchema = value; } }

		/// <summary>
		///   The Db catalog (database) where the target object resides.
		/// </summary>
		public string TargetCatalog { get; set; }

		/// <summary>
		///   The connection name where the type's data resides.
		/// </summary>
		public string ConnectionName { get { return String.IsNullOrEmpty(_connectionName) ? Mappings.Instance.DefaultConnection : _connectionName; } set { _connectionName = value; } }

		/// <summary>
		///   The ORM strategy.
		/// </summary>
		public MappingStrategy Strategy { get; internal set; }

		/// <summary>
		///   The full name of the primary underlying database object.
		/// </summary>
		public string DbObjectReference
		{
			get
			{
				return String.IsNullOrEmpty(TargetSchema)
					? QuoteObjectName(TargetObject)
					: String.Concat(QuoteObjectName(TargetSchema), '.', QuoteObjectName(TargetObject));
			}
		}

		/// <summary>
		///   The columns that are mapped to the object.
		/// </summary>
		public IEnumerable<ColumnMapping> Columns { get { return _columns.AsReadOnly(); } }

		/// <summary>
		///   The columns that are mapped to the object.
		/// </summary>
		public IEnumerable<ColumnMapping> DeclaredColumns { get { return _declaredColumns.AsReadOnly(); } }

		/// <summary>
		/// Gets member info for each member participating in the database mapping.
		/// </summary>
		public IEnumerable<MemberInfo> ParticipatingMembers
		{
			get
			{
				return Columns.Select(c => c.Member)
											.Concat(_collections.Select(kvp => kvp.Value.LocalMember))
											.ToReadOnly();
			}
		}

		/// <summary>
		///   The collections that are mapped to the object.
		/// </summary>
		public IEnumerable<CollectionMapping> Collections
		{
			get
			{
				var res = new List<CollectionMapping>();
				foreach (var it in _baseTypes)
				{
					res.AddRange(it.DeclaredCollections);
				}
				res.AddRange(_collections.Values);
				return res.AsReadOnly();
			}
		}

		/// <summary>
		///   The collections that are mapped to the object.
		/// </summary>
		public IEnumerable<CollectionMapping> DeclaredCollections { get { return _collections.Values.ToReadOnly(); } }

		/// <summary>
		/// Gets the type's database object name quoted for the underlying database's script syntax.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public string QuoteObjectName(string name)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			if (String.IsNullOrEmpty(ConnectionName))
			{
				return name;
			}
			var helper = GetDbProviderHelper();
			return (helper != null) ? helper.QuoteObjectName(name) : name;
		}

		/// <summary>
		///   The data model's dependencies.
		/// </summary>
		public IEnumerable<Dependency> Dependencies
		{
			get
			{
				var res = new List<Dependency>();
				foreach (var it in _baseTypes)
				{
					res.AddRange(it.DeclaredDependencies);
				}
				res.AddRange(_dependencies);
				return res.AsReadOnly();
			}
		}

		/// <summary>
		///   The data model's declared dependencies.
		/// </summary>
		public IEnumerable<Dependency> DeclaredDependencies { get { return _dependencies.ToArray(); } }

		/// <summary>
		///   Indicates whether the mapping is complete.
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public IMapping OnCompleted(Action<IMapping> action)
		{
			if (IsComplete)
			{
				action(this);
			}
			else
			{
				_whenCompleted.Enqueue(action);
				if (IsComplete)
				{
					Action<IMapping> a;
					while (_whenCompleted.TryDequeue(out a))
					{
						a(this);
					}
				}
			}
			return this;
		}

		/// <summary>
		///   Gets the mapping's model binder.
		/// </summary>
		/// <returns></returns>
		public IDataModelBinder GetBinder()
		{
			if (_binder != null) return _binder;
			if (IdentityKey.KeyType == null)
			{
				throw new InvalidOperationException(String.Concat("IdentityKey not defined for type: ",
					RuntimeType.GetReadableSimpleName(), "."));
			}
			var helper = GetDbProviderHelper();
			var get = typeof (DbProviderHelper).MatchGenericMethod("GetModelBinder", 2, typeof (IDataModelBinder<,>),
				typeof (IMapping<>))
				.MakeGenericMethod(typeof (TModel), IdentityKey.KeyType);
			_binder = (IDataModelBinder) get.Invoke(helper, new object[] {this});
			return _binder;
		}

		/// <summary>
		///   Notifies the mapping of a subtype.
		/// </summary>
		/// <param name="mapping"></param>
		public void NotifySubtype(IMapping mapping)
		{
			var ht = typeof(IHierarchyMapping<TModel>).MatchGenericMethod("NotifySubtype", 1, typeof(void), typeof(IMapping<>))
																								.MakeGenericMethod(mapping.RuntimeType);
			ht.Invoke(Hierarchy, new object[] {mapping});
		}

		#endregion

		public int Revision
		{
			get { return Thread.VolatileRead(ref _revision); }
		}

		public Type IdentityKeyType
		{
			get { return (HasIdentity) ? IdentityKey.KeyType : null; }
		}
	}
}