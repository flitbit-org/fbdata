#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections;
using System.Linq;
using FlitBit.Core.Factory;
using FlitBit.Core.Meta;

namespace FlitBit.Data.Meta
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
	public sealed class MapEntityAttribute : AutoImplementedAttribute
	{
		public MapEntityAttribute()
			: base(InstanceScopeKind.OnDemand)
		{}

		public MapEntityAttribute(EntityBehaviors behaviors)
			: this(null, null, null, default(MappingStrategy), behaviors)
		{}

		public MapEntityAttribute(string targetSchema)
			: this(targetSchema, null, null, default(MappingStrategy), EntityBehaviors.Default)
		{}

		public MapEntityAttribute(string targetSchema, EntityBehaviors behaviors)
			: this(targetSchema, null, null, default(MappingStrategy), behaviors)
		{}

		public MapEntityAttribute(string targetSchema, EntityBehaviors behaviors, MappingStrategy strategy)
			: this(targetSchema, null, null, strategy, behaviors)
		{ }

		public MapEntityAttribute(string targetSchema, string targetName)
			: this(targetSchema, targetName, null, default(MappingStrategy), EntityBehaviors.Default)
		{}

		public MapEntityAttribute(string targetSchema, string targetName, EntityBehaviors behaviors)
			: this(targetSchema, targetName, null, default(MappingStrategy), behaviors)
		{}

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

		public EntityBehaviors Behaviors { get; private set; }
		public string ConnectionName { get; set; }
		public MappingStrategy Strategy { get; private set; }
		public string TargetName { get; set; }
		public string TargetSchema { get; set; }

		/// <summary>
		///   Implements the stereotypical DataModel behavior for interfaces of type T.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <param name="factory">the requesting factory</param>
		/// <param name="complete">callback invoked with the implementation type or the type's factory function.</param>
		/// <returns>
		///   <em>true</em> if the data model was generated; otherwise <em>false</em>.
		/// </returns>
		public override bool GetImplementation<T>(IFactory factory, Action<Type, Func<T>> complete)
		{
			if (typeof(T).IsDefined(typeof(MapEntityAttribute), true))
			{
				complete(DataModels.ConcreteType<T>(), null);
				return true;
			}
			return false;
		}

		internal void PrepareMapping<T>(Mapping<T> mapping, Type declaringType)
		{
			if (declaringType == mapping.RuntimeType)
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
			}

			var mapAllProperties = this.Behaviors.HasFlag(EntityBehaviors.MapAllProperties);
			foreach (var p in declaringType.GetProperties())
			{
				var mapColumn = (MapColumnAttribute) p.GetCustomAttributes(typeof(MapColumnAttribute), false)
																							.SingleOrDefault();
				if (mapColumn != null)
				{
					mapColumn.PrepareMapping(mapping, p);
				}
				else
				{
					if (p.IsDefined(typeof(MapInplaceColumnsAttribute), false))
					{
						var meta = (MapInplaceColumnsAttribute) p.GetCustomAttributes(typeof(MapInplaceColumnsAttribute), false)
																										.Single();
						meta.PrepareMapping(mapping, p);
					}
					else if (p.IsDefined(typeof(MapCollectionAttribute), false))
					{
						var mapColl = (MapCollectionAttribute) p.GetCustomAttributes(typeof(MapCollectionAttribute), false)
																										.Single();
						mapping.MapCollectionFromMeta(p, mapColl);
					}
					else if (mapAllProperties && !typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
					{
						mapColumn = MapColumnAttribute.DefineOnProperty<T>(p);
						mapColumn.PrepareMapping(mapping, p);
					}
				}
			}
		}
	}
}