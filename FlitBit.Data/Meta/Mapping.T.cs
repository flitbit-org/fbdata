#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using FlitBit.Core;
using FlitBit.Emit;
using FlitBit.Wireup;

namespace FlitBit.Data.Meta
{
	public partial class Mapping<T> : IMapping
	{	
		readonly Dictionary<string, ColumnMapping> _columns = new Dictionary<string, ColumnMapping>();
		readonly Dictionary<string, CollectionMapping<T>> _collections = new Dictionary<string, CollectionMapping<T>>();
		readonly List<Dependency> _dependencies = new List<Dependency>();
		readonly ConcurrentQueue<Action> _whenCompleted = new ConcurrentQueue<Action>();

		int _columnCount = 0;
		IdentityMapping<T> _identity;
		NaturalKeyMapping<T> _naturalKey;
		List<string> _errors;
		string _connectionName;
		string _targetSchema;
		
		internal Mapping()
		{
			_identity = new IdentityMapping<T>(this);
			_naturalKey = new NaturalKeyMapping<T>(this);
			var name = typeof(T).Name;
			if (name.Length > 1 && name[0] == 'I' && Char.IsUpper(name[1]))
				name = name.Substring(1);
			this.TargetObject = name;
		}
		
		public Type RuntimeType { get { return typeof(T); } }

		/// <summary>
		/// Indicates whether the mapping has been completed.
		/// </summary>
		public bool IsComplete { get; private set; }

		/// <summary>
		/// Indicates whether the entity is an enum type.
		/// </summary>
		public bool IsEnum { get { return Behaviors.HasFlag(EntityBehaviors.MapEnum); } }

		/// <summary>
		/// The Db object to which type T maps; either a table or view.
		/// </summary>
		public string TargetObject { get; set; }
		/// <summary>
		/// The Db schema where the target object resides.
		/// </summary>
		public string TargetSchema
		{
			get { return String.IsNullOrEmpty(_targetSchema) ? Mapping.Instance.DefaultSchema : _targetSchema; }
			set { _targetSchema = value; }
		}
		/// <summary>
		/// The Db catalog (database) where the target object resides.
		/// </summary>
		public string TargetCatalog { get; set; }
		/// <summary>
		/// The connection name where the type's data resides.
		/// </summary>
		public string ConnectionName
		{
			get { return String.IsNullOrEmpty(_connectionName) ? Mapping.Instance.DefaultConnection : _connectionName; }
			set { _connectionName = value; }
		}
		/// <summary>
		/// The ORM strategy.
		/// </summary>
		public MappingStrategy Strategy { get; private set; }

		public string DbObjectReference
		{
			get
			{
				return String.IsNullOrEmpty(TargetSchema)
					? QuoteObjectNameForSQL(TargetObject)
					: String.Concat(QuoteObjectNameForSQL(TargetSchema), '.', QuoteObjectNameForSQL(TargetObject));
			}
		}
		public bool HasIdentity
		{
			get { return _identity.Columns.Count() > 0; }
		}

		/// <summary>
		/// The columns that are mapped to the object.
		/// </summary>
		public IEnumerable<ColumnMapping> Columns
		{
			get { return _columns.Values.OrderBy(c => c.Ordinal).ToReadOnly(); }
		}

		public IEnumerable<MemberInfo> ParticipatingMembers
		{
			get
			{
				return _columns.Select(kvp => kvp.Value.Member)
					.Concat(_collections.Select(kvp => kvp.Value.Member))
					.ToReadOnly();
			}
		}

		/// <summary>
		/// The collections that are mapped to the object.
		/// </summary>
		public IEnumerable<CollectionMapping<T>> Collections
		{
			get { return _collections.Values.ToReadOnly(); }
		}

		/// <summary>
		/// Maps a column to a member (property or field) of the object.
		/// The property or field indicated will serve as the column definition.
		/// </summary>
		/// <param name="expression">An expression that identifies the member
		/// upon which a column will be mapped.</param>
		/// <returns>A ColumnMapping object for further refinement of the column's
		/// definition.</returns>
		public ColumnMapping<T> Column(Expression<Func<T, object>> expression)
		{
			Contract.Requires(expression != null);

			MemberInfo member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member");

			var name = member.Name;

			ColumnMapping col;
			lock (_columns)
			{
				if (!_columns.TryGetValue(name, out col))
				{
					_columns.Add(name, col = ColumnMapping.FromMember<T>(this, member, Interlocked.Increment(ref _columnCount)));
				}
			}
			return (ColumnMapping<T>)col;
		}
		internal ColumnMapping<T> DefineColumn(PropertyInfo p)
		{
			var name = p.Name;

			ColumnMapping col;
			lock (_columns)
			{
				if (!_columns.TryGetValue(name, out col))
				{
					_columns.Add(name, col = ColumnMapping.FromMember<T>(this, p, Interlocked.Increment(ref _columnCount)));
				}
			}
			return (ColumnMapping<T>)col;
		}

		public CollectionMapping<T> Collection(Expression<Func<T, object>> expression)
		{
			Contract.Requires(expression != null);

			MemberInfo member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member");

			var name = member.Name;

			CollectionMapping<T> col;
			lock (_collections)
			{
				if (!_collections.TryGetValue(name, out col))
				{
					_collections.Add(name, col = new CollectionMapping<T>(this, member));
				}
			}
			return (CollectionMapping<T>)col;
		}
		CollectionMapping<T> DefineCollection(PropertyInfo property)
		{
			var name = property.Name;

			CollectionMapping<T> col;
			lock (_collections)
			{
				if (!_collections.TryGetValue(name, out col))
				{
					_collections.Add(name, col = new CollectionMapping<T>(this, property));
				}
			}
			return (CollectionMapping<T>)col;
		}

		/// <summary>
		/// Sets the database object name
		/// </summary>
		/// <param name="name">Name of the database object where data is stored for
		/// the type.</param>
		/// <returns></returns>
		public Mapping<T> WithName(string name)
		{
			Contract.Requires(name != null);
			Contract.Requires(name.Length > 0);

			this.TargetObject = name;
			return this;
		}

		public Mapping<T> InSchema(string schema)
		{
			Contract.Requires(schema != null);
			Contract.Requires(schema.Length > 0);

			this.TargetSchema = schema;
			return this;
		}

		public Mapping<T> ReferencesType<U>(Expression<Func<T, U, bool>> expression)
		{
			return this;
		}
				
		public Mapping<T> MapAllOperations()
		{
			return MapAllOperations(MappingStrategy.OneClassOneTable);
		}
		public Mapping<T> MapAllOperations(MappingStrategy strategy)
		{
			this.Strategy = strategy;
			var errors = new List<string>();
			var warnings = new List<string>();

			if (String.IsNullOrEmpty(this.ConnectionName))
				errors.Add("ConnectionName has not been configured.");

			if (String.IsNullOrEmpty(this.TargetCatalog))
				warnings.Add("TargetCatalog has not been configured; the catalog will be determined by the connection string.");
			if (String.IsNullOrEmpty(this.TargetSchema))
				warnings.Add("TargetSchema has not been configured; none will be used.");
			if (String.IsNullOrEmpty(this.TargetObject))
				warnings.Add(String.Concat("TargetObject has not been configured; the type name will be used: ", typeof(T).Name));

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

		public Mapping<T> UsesConnection(string connection)
		{
			Contract.Requires(connection != null);
			Contract.Requires(connection.Length > 0);

			this.ConnectionName = connection;
			return this;
		}

		public string QuoteObjectNameForSQL(string name)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			if (String.IsNullOrEmpty(ConnectionName))
			{
				return name;
			}
			else
			{
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(ConnectionName);

				return (helper != null) ? helper.QuoteObjectName(name) : name;
			}
		}

		/// <summary>
		/// Gets the mapped type's identity mapping.
		/// </summary>
		public IdentityMapping<T> Identity { get { return _identity; } }

		public NaturalKeyMapping<T> NaturalKey { get { return _naturalKey; } }

		public IEnumerable<string> Errors
		{
			get { return (_errors == null) ? new String[0] : _errors.ToArray(); }
		}

		public IEnumerable<Dependency> Dependencies
		{
			get { return _dependencies.ToArray(); }
		}

		/// <summary>
		/// Completes the mapping.
		/// </summary>
		/// <returns></returns>
		public Mapping End()
		{
			if (!HasIdentity)
			{ // Try to discover identity columns from column definitions.
				foreach (var c in Columns.Where(c => c.Behaviors.HasFlag(ColumnBehaviors.Identity)))
				{
					Identity.AddColumn(c);
				}
			}
			this.MarkComplete();
			return Mapping.Instance;
		}

		private void MarkComplete()
		{
			this.IsComplete = true;
			Action a;
			while (_whenCompleted.TryDequeue(out a))
				a();
		}

		internal Mapping<T> InitFromMetadata()
		{
			var typ = typeof(T);
			var mod = typ.Module;
			var asm = typ.Assembly;
			WireupCoordinator.Instance.WireupDependencies(asm);
			// module, then assembly - module takes precedence
			if (mod.IsDefined(typeof(MapConnectionAttribute), false))
			{
				var conn = (MapConnectionAttribute)mod
					.GetCustomAttributes(typeof(MapConnectionAttribute), false).First();
				conn.PrepareMapping(this);
			}
			else if (asm.IsDefined(typeof(MapConnectionAttribute), false))
			{
				var conn = (MapConnectionAttribute)asm
					.GetCustomAttributes(typeof(MapConnectionAttribute), false).First();
				conn.PrepareMapping(this);
			}
			if (mod.IsDefined(typeof(MapSchemaAttribute), false))
			{
				var schema = (MapSchemaAttribute)mod
					.GetCustomAttributes(typeof(MapSchemaAttribute), false).First();
				schema.PrepareMapping(this);
			}
			else if (asm.IsDefined(typeof(MapSchemaAttribute), false))
			{
				var schema = (MapSchemaAttribute)asm
					.GetCustomAttributes(typeof(MapSchemaAttribute), false).First();
				schema.PrepareMapping(this);
			}
			if (typ.IsDefined(typeof(MapEntityAttribute), false))
			{
				var entity = (MapEntityAttribute)typ
					.GetCustomAttributes(typeof(MapEntityAttribute), false).First();
				entity.PrepareMapping(this);
			}
			return this;
		}

		internal void MapCollectionFromMeta(PropertyInfo p, MapCollectionAttribute mapColl)
		{
			var elmType = p.PropertyType.FindElementType();
			if (!Mapping.ExistsFor(elmType))
				new MappingException(String.Concat(typeof(T).Name, ": reference collection must be mapped over other mapped types: ", p.Name));

			MemberInfo refColumn = null;
			if (mapColl.References == null || mapColl.References.Count() == 0)
			{
				refColumn = InferCollectionReferenceTargetMember(p, elmType);
			}
			else
			{
				var refColumnName = mapColl.References.Select(s => s.Trim()).Where(s => !String.IsNullOrEmpty(s)).First();
				refColumn = elmType.GetProperty(refColumnName);
				if (refColumn == null)
					throw new InvalidOperationException(String.Concat(typeof(T).Name, ": relationship property doesn't exist on the target type: ", refColumnName));
			}																																																																										
			
			var coll = DefineCollection(p);
			coll.ReferenceJoinMember = refColumn;
			coll.ReferenceBehaviors = mapColl.ReferenceBehaviors;
		}

		internal MemberInfo InferCollectionReferenceTargetMember(MemberInfo p, Type elementType)
		{
			var elmMapping = Mapping.AccessMappingFor(elementType);
			AddDependency(elmMapping, DependencyKind.Soft, p);

			var foreign_object_id = elmMapping.GetPreferredReferenceColumn();
			if (foreign_object_id == null)
				throw new MappingException(String.Concat("Relationship not defined between ", typeof(T).Name, ".", p.Name, " and the referenced type: ", elementType.Name));

			return foreign_object_id.Member;			
		}

		internal void MapColumnFromMeta(PropertyInfo p, MapColumnAttribute mapColumn)
		{
			var column = this.DefineColumn(p);
			if (!String.IsNullOrEmpty(mapColumn.TargetName))
				column.WithTargetName(mapColumn.TargetName);
			var behaviors = mapColumn.Behaviors;
			if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				behaviors |= ColumnBehaviors.Nullable;
			}
			column.WithBehaviors(behaviors);
			if (mapColumn.Length > 0)
				column.WithVariableLength(mapColumn.Length);
			if (column.Behaviors.HasFlag(ColumnBehaviors.Identity))
			{
				Identity.AddColumn(column);
			}

			if (Mapping.ExistsFor(p.PropertyType))
			{
				var foreignMapping = Mapping.AccessMappingFor(p.PropertyType);
				var foreignColumn = default(ColumnMapping);
				AddDependency(foreignMapping, DependencyKind.Direct, column.Member);

				if (mapColumn.References == null || mapColumn.References.Count() == 0)
				{
					foreignColumn = foreignMapping.GetPreferredReferenceColumn();
					if (foreignColumn == null)
						throw new MappingException(String.Concat("Relationship not defined between ", typeof(T).Name, ".", p.Name, " and the referenced type: ", p.PropertyType.Name));

					mapColumn.References = new string[] { foreignColumn.Member.Name };
				}
				else
				{
					// Only 1 reference column for now.
					foreignColumn = foreignMapping.Columns.Where(c => c.Member.Name == mapColumn.References.First()).FirstOrDefault();
				}

				if (foreignColumn == null)
					throw new InvalidOperationException(String.Concat("Property '", p.Name, "' references an entity but a relationship cannot be determined."));
				column.DefineReference(foreignColumn, mapColumn.ReferenceBehaviors);
			}
		}

		public EntityBehaviors Behaviors { get; internal set; }
				
		internal void AddDependency(IMapping target, DependencyKind kind, MemberInfo member)
		{
			lock (_dependencies)
			{
				var dep = _dependencies.Find(d => d.Target == target);
				if (dep == null)
				{
					_dependencies.Add(new Dependency(kind, this, member, target).CalculateDependencyKind());
				}
			}
		}

		public IMapping Completed(Action action)
		{
			if (IsComplete) action();
			else
			{
				_whenCompleted.Enqueue(action);
				if (IsComplete)
				{
					Action a;
					while (_whenCompleted.TryDequeue(out a))
						a();
				}
			}
			return this;
		}
	}
}
