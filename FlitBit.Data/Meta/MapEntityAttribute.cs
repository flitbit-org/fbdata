#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;
using FlitBit.Emit;
using FlitBit.Core.Meta;
using FlitBit.Core.Factory;

namespace FlitBit.Data.Meta
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
	public sealed class MapEntityAttribute: AutoImplementedAttribute
	{	
		public MapEntityAttribute() : base(InstanceScopeKind.OnDemand) { }
		public MapEntityAttribute(EntityBehaviors behaviors)
			: this(null, null, null, MappingStrategy.OneClassOneTable, behaviors)
		{ 
		}		
		public MapEntityAttribute(string targetSchema)
			: this(targetSchema, null, null, MappingStrategy.OneClassOneTable, EntityBehaviors.Default)
		{
		}
		public MapEntityAttribute(string targetSchema, EntityBehaviors behaviors)
			: this(targetSchema, null, null, MappingStrategy.OneClassOneTable, behaviors)
		{
		}
		public MapEntityAttribute(string targetSchema, string targetName)
			: this(targetSchema, targetName, null, MappingStrategy.OneClassOneTable, EntityBehaviors.Default)
		{
		}
		public MapEntityAttribute(string targetSchema, string targetName, EntityBehaviors behaviors)
			: this(targetSchema, targetName, null, MappingStrategy.OneClassOneTable, behaviors)
		{
		}
		public MapEntityAttribute(string targetSchema, string targetName, 
			string connectionName, MappingStrategy strategy, EntityBehaviors behaviors)
			: base(InstanceScopeKind.OnDemand)
		{
			this.TargetName = targetName;
			this.TargetSchema = targetSchema;
			this.ConnectionName = connectionName;
			this.Strategy = strategy;
			this.Behaviors = behaviors;
		}
		
		public string TargetName { get; set; }
		public string TargetSchema { get; set; }
		public string ConnectionName { get; set; }
		public MappingStrategy Strategy { get; private set; }
		public EntityBehaviors Behaviors { get; private set; }

		internal void PrepareMapping<T>(Mapping<T> mapping)
		{
			if (!String.IsNullOrEmpty(TargetName))
			{
				mapping.WithName(TargetName);
			}
			if (!String.IsNullOrEmpty(ConnectionName))
			{
				mapping.UsesConnection(ConnectionName);
			}
			if (!String.IsNullOrEmpty(TargetSchema))
			{
				mapping.InSchema(TargetSchema);
			}
			mapping.Behaviors = this.Behaviors;
			
			foreach (var type in typeof(T).GetTypeHierarchyInDeclarationOrder()
				.Except(new Type[] 
				{ 
					typeof(Object), 
					typeof(INotifyPropertyChanged) 
				}))
			{
				var mapAllProperties = this.Behaviors.HasFlag(EntityBehaviors.MapAllProperties);
				foreach (var p in type.GetProperties())
				{
					var mapColumn = (MapColumnAttribute)p.GetCustomAttributes(typeof(MapColumnAttribute), false).SingleOrDefault();
					if (mapColumn == null
						&& mapAllProperties
						&& !typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
					{
						mapColumn = MapColumnAttribute.DefineOnProperty<T>(p);
					}
					if (mapColumn != null)
					{
						mapColumn.PrepareMapping(mapping, p);
					}
					var mapColl = (MapCollectionAttribute)p.GetCustomAttributes(typeof(MapCollectionAttribute), false).SingleOrDefault();
					if (mapColl != null)
					{
						mapping.MapCollectionFromMeta(p, mapColl);
					}
				}
			}
			if (mapping.Behaviors.HasFlag(EntityBehaviors.MapEnum))
			{
				var idcol = mapping.Identity.Columns.Where(c => c.RuntimeType.IsEnum).SingleOrDefault();
				if (idcol == null)
					throw new MappingException(String.Concat("Entity type ", typeof(T).Name, " declares behavior EntityBehaviors.MapEnum but the enum type cannot be determined. Specify an identity column of enum type."));
				var namecol = mapping.Columns.Where(c => c.RuntimeType == typeof(String) && c.IsAlternateKey).FirstOrDefault();
				if (namecol == null)
					throw new MappingException(String.Concat("Entity type ", typeof(T).Name, " declares behavior EntityBehaviors.MapEnum but a column to hold the enum name cannot be determined. Specify a string column with alternate key behavior."));
				var names = Enum.GetNames(idcol.Member.GetTypeOfValue());
				namecol.VariableLength = names.Max(n => n.Length);				
			}
		}

		public override bool GetImplementation<T>(IFactory factory, Action<Type, Func<T>> complete)
		{
			throw new NotImplementedException();
		}		
	}
}
