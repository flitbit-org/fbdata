#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Core.Factory;
using FlitBit.Core.Meta;
using FlitBit.Data.DataModel;
using FlitBit.Emit;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

namespace FlitBit.Data.Meta
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
	public sealed class MapEntityAttribute : WireupTaskAttribute
	{
		public MapEntityAttribute()
			: base(WireupPhase.Tasks)
		{ }

		public MapEntityAttribute(EntityBehaviors behaviors)
			: this(null, null, null, default(MappingStrategy), behaviors)
		{ }

		public MapEntityAttribute(string targetSchema)
			: this(targetSchema, null, null, default(MappingStrategy), EntityBehaviors.Default)
		{ }

		public MapEntityAttribute(string targetSchema, EntityBehaviors behaviors)
			: this(targetSchema, null, null, default(MappingStrategy), behaviors)
		{ }

		public MapEntityAttribute(string targetSchema, EntityBehaviors behaviors, MappingStrategy strategy)
			: this(targetSchema, null, null, strategy, behaviors)
		{ }

		public MapEntityAttribute(string targetSchema, string targetName)
			: this(targetSchema, targetName, null, default(MappingStrategy), EntityBehaviors.Default)
		{ }

		public MapEntityAttribute(string targetSchema, string targetName, EntityBehaviors behaviors)
			: this(targetSchema, targetName, null, default(MappingStrategy), behaviors)
		{ }

		public MapEntityAttribute(string targetSchema, string targetName,
			string connectionName, MappingStrategy strategy, EntityBehaviors behaviors)
			: base(WireupPhase.Tasks)
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
				mapping.Strategy = this.Strategy;
			}

			var mapAllProperties = this.Behaviors.HasFlag(EntityBehaviors.MapAllProperties);
			foreach (var p in declaringType.GetProperties())
			{
				var mapColumn = (MapColumnAttribute)p.GetCustomAttributes(typeof(MapColumnAttribute), false)
																							.SingleOrDefault();
				if (mapColumn != null)
				{
					mapColumn.PrepareMapping(mapping, p);
				}
				else
				{
					if (p.IsDefined(typeof(MapInplaceColumnsAttribute), false))
					{
						var meta = (MapInplaceColumnsAttribute)p.GetCustomAttributes(typeof(MapInplaceColumnsAttribute), false)
																										.Single();
						meta.PrepareMapping(mapping, p);
					}
					else if (p.IsDefined(typeof(MapCollectionAttribute), false))
					{
						var mapColl = (MapCollectionAttribute)p.GetCustomAttributes(typeof(MapCollectionAttribute), false)
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

		/// <summary>
		/// Called by the base class upon execution. Derived classes should
		///               provide an implementation that performs the wireup logic.
		/// </summary>
		protected override void PerformTask(IWireupCoordinator coordinator, Wireup.Recording.WireupContext context)
		{
		}
	}

	internal static class EntityWireupObserver
	{
		public static readonly Guid WireupObserverKey = new Guid("C24627E8-ECA0-4ECA-93E0-AD907461CD26");

		static readonly MethodInfo ConcreteTypeMethod = typeof(DataModelEmitter).MatchGenericMethod("ConcreteType",
																																																	BindingFlags.Static | BindingFlags.NonPublic, 1, typeof(Type));

		static readonly MethodInfo RegisterMethod = typeof(IFactory).MatchGenericMethod("RegisterImplementationType", 2,
																																										typeof(void));


		static readonly IWireupObserver SingletonObserver = new DtoObserver();

		public static IWireupObserver Observer { get { return SingletonObserver; } }

		class DtoObserver : IWireupObserver
		{
			#region IWireupObserver Members

			public void NotifyWireupTask(IWireupCoordinator coordinator, WireupTaskAttribute task, Type target)
			{
				var cra = task as MapEntityAttribute;
				if (cra != null && target != null)
				{
					var concreteMethod = ConcreteTypeMethod.MakeGenericMethod(target);
					var concrete = (Type)concreteMethod.Invoke(null, null);
					var reg = RegisterMethod.MakeGenericMethod(target, concrete);
					reg.Invoke(FactoryProvider.Factory, null);
				}
			}

			/// <summary>
			///   Gets the observer's key.
			/// </summary>
			public Guid ObserverKey { get { return WireupObserverKey; } }

			#endregion
		}
	}
}