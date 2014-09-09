#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using FlitBit.Data;
using FlitBit.Data.Catalog;
using FlitBit.Data.Meta;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using FlitBit.Wireup.Recording;

[assembly: HookWirupCoordinatorsTask]

namespace FlitBit.Data
{
    /// <summary>
    ///     Wires this module.
    /// </summary>
    public class HookWirupCoordinatorsTask : WireupTaskAttribute
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public HookWirupCoordinatorsTask()
            : base(WireupPhase.BeforeTasks) { }

        /// <summary>
        ///     Called by the base class upon execution. Derived classes should
        ///     provide an implementation that performs the wireup logic.
        /// </summary>
        protected override void PerformTask(IWireupCoordinator coordinator, WireupContext context)
        {
            // Attach the DataModelAttribute observer...
            coordinator.RegisterObserver(StaticCatalog.Observer);
            // Attach the MapEntityAttribute observer...
            coordinator.RegisterObserver(EntityWireupObserver.Observer);
        }
    }
}