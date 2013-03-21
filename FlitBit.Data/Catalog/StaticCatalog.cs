using System;
using FlitBit.Data.Meta;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

namespace FlitBit.Data.Catalog
{
	public static class StaticCatalog
	{
		public static readonly Guid ContainerWireupObserverKey = new Guid("693183CA-1D1E-4186-B84F-082FE9D8B551");

		class DataModelObserver: IWireupObserver
		{
			/// <summary>
			/// Called by coordinators to notify observers of wireup tasks.
			/// </summary>
			/// <param name="coordinator"/><param name="task"/><param name="target"/>
			public void NotifyWireupTask(IWireupCoordinator coordinator, WireupTaskAttribute task, Type target)
			{
				var cra = task as DataModelAttribute;
				if (cra != null && target != null)
				{
					var mapping = Mappings.AccessMappingFor(target);
					var preparer = (PrepareDataMapping) Activator.CreateInstance(cra.PrepareDataMappingType);
					preparer.UntypedPrepareMapping(mapping);
				}
			}

			/// <summary>
			/// Gets the observer's key.
			/// </summary>
			public Guid ObserverKey { get { return ContainerWireupObserverKey; } }
		}

		readonly static IWireupObserver __observer = new DataModelObserver();

		public static IWireupObserver Observer { get { return __observer; } }
	}

}
