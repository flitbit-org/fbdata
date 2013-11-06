#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Core.Factory;
using FlitBit.Data.DataModel;
using FlitBit.Emit;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using FlitBit.Wireup.Recording;

namespace FlitBit.Data.Meta
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
	public sealed class MapEntityAttribute : WireupTaskAttribute
	{
		public MapEntityAttribute()
			: base(WireupPhase.Tasks)
		{
		}

		public MapEntityAttribute(EntityBehaviors behaviors)
			: this(null, null, null, default(MappingStrategy), behaviors)
		{
		}

		public MapEntityAttribute(EntityBehaviors behaviors, MappingStrategy strategy)
			: this(null, null, null, strategy, behaviors)
		{
		}

		public MapEntityAttribute(string targetSchema)
			: this(targetSchema, null, null, default(MappingStrategy), EntityBehaviors.Default)
		{
		}

		public MapEntityAttribute(string targetSchema, EntityBehaviors behaviors)
			: this(targetSchema, null, null, default(MappingStrategy), behaviors)
		{
		}

		public MapEntityAttribute(string targetSchema, EntityBehaviors behaviors, MappingStrategy strategy)
			: this(targetSchema, null, null, strategy, behaviors)
		{
		}

		public MapEntityAttribute(string targetSchema, string targetName)
			: this(targetSchema, targetName, null, default(MappingStrategy), EntityBehaviors.Default)
		{
		}

		public MapEntityAttribute(string targetSchema, string targetName, EntityBehaviors behaviors)
			: this(targetSchema, targetName, null, default(MappingStrategy), behaviors)
		{
		}

		public MapEntityAttribute(string targetSchema, string targetName,
			string connectionName, MappingStrategy strategy, EntityBehaviors behaviors)
			: base(WireupPhase.Tasks)
		{
			TargetName = targetName;
			TargetSchema = targetSchema;
			ConnectionName = connectionName;
			Strategy = strategy;
			Behaviors = behaviors;
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
				mapping.Behaviors = Behaviors;
				mapping.Strategy = Strategy;
			}
			string name = typeof (T).Name;
			if (name.Length > 1 && name[0] == 'I' && Char.IsUpper(name[1]))
			{
				name = name.Substring(1);
			}
			mapping.TargetObject = name;

			bool mapAllProperties = Behaviors.HasFlag(EntityBehaviors.MapAllProperties);
			foreach (PropertyInfo p in declaringType.GetProperties())
			{
				var mapColumn = (MapColumnAttribute) p.GetCustomAttributes(typeof (MapColumnAttribute), false)
					.SingleOrDefault();
				if (mapColumn != null)
				{
					mapColumn.PrepareMapping(mapping, p);
				}
				else
				{
					if (p.IsDefined(typeof (MapInplaceColumnsAttribute), false))
					{
						var meta = (MapInplaceColumnsAttribute) p.GetCustomAttributes(typeof (MapInplaceColumnsAttribute), false)
							.Single();
						meta.PrepareMapping(mapping, p);
					}
					else if (p.IsDefined(typeof (MapCollectionAttribute), false))
					{
						var mapColl = (MapCollectionAttribute) p.GetCustomAttributes(typeof (MapCollectionAttribute), false)
							.Single();
						mapping.MapCollectionFromMeta(p, mapColl);
					}
					else if (mapAllProperties && !typeof (IEnumerable).IsAssignableFrom(p.PropertyType))
					{
						mapColumn = MapColumnAttribute.DefineOnProperty<T>(p);
						mapColumn.PrepareMapping(mapping, p);
					}
				}
			}
		}

		/// <summary>
		///   Called by the base class upon execution. Derived classes should
		///   provide an implementation that performs the wireup logic.
		/// </summary>
		protected override void PerformTask(IWireupCoordinator coordinator, WireupContext context)
		{
		}
	}

	internal static class EntityWireupObserver
	{
		public static readonly Guid WireupObserverKey = new Guid("C24627E8-ECA0-4ECA-93E0-AD907461CD26");

		private static readonly MethodInfo ConcreteTypeMethod = typeof (DataModelEmitter).MatchGenericMethod("ConcreteType",
			BindingFlags.Static | BindingFlags.NonPublic, 1, typeof (Type));

		private static readonly MethodInfo RegisterMethod = typeof (IFactory).MatchGenericMethod(
			"RegisterImplementationType", 2,
			typeof (void));


		private static readonly IWireupObserver SingletonObserver = new EntityObserver();

		public static IWireupObserver Observer
		{
			get { return SingletonObserver; }
		}

		private class EntityObserver : IWireupObserver
		{
			#region IWireupObserver Members

			public void NotifyWireupTask(IWireupCoordinator coordinator, WireupTaskAttribute task, Type target)
			{
				var cra = task as MapEntityAttribute;
				if (cra != null && target != null)
				{
					var concreteType = typeof (DataModel<>).MakeGenericType(target)
						.GetProperty("ConcreteType", BindingFlags.Public | BindingFlags.Static);
					var concrete = (Type) concreteType.GetGetMethod().Invoke(null, null);
					MethodInfo reg = RegisterMethod.MakeGenericMethod(target, concrete);
					reg.Invoke(FactoryProvider.Factory, null);
				}
			}

			/// <summary>
			///   Gets the observer's key.
			/// </summary>
			public Guid ObserverKey
			{
				get { return WireupObserverKey; }
			}

			#endregion
		}
	}
}