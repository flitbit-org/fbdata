#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.Data.Meta;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

namespace FlitBit.Data.Catalog
{
    /// <summary>
    ///     Static utilities.
    /// </summary>
    public static class StaticCatalog
    {
        /// <summary>
        ///     The data model observer's wirup observer key.
        /// </summary>
        public static readonly Guid WireupObserverKey = new Guid("693183CA-1D1E-4186-B84F-082FE9D8B551");

        class DataModelObserver : IWireupObserver
        {
            /// <summary>
            ///     Called by coordinators to notify observers of wireup tasks.
            /// </summary>
            /// <param name="coordinator" />
            /// <param name="task" />
            /// <param name="target" />
            public void NotifyWireupTask(IWireupCoordinator coordinator, WireupTaskAttribute task, Type target)
            {
                var cra = task as DataModelAttribute;
                if (cra != null
                    && target != null)
                {
                    var mapping = Mappings.AccessMappingFor(target);
                    var preparer = (PrepareDataMapping)Activator.CreateInstance(cra.PrepareDataMappingType);
                    preparer.UntypedPrepareMapping(mapping);
                }
            }

            /// <summary>
            ///     Gets the observer's key.
            /// </summary>
            public Guid ObserverKey { get { return WireupObserverKey; } }
        }

        static readonly IWireupObserver __observer = new DataModelObserver();

        /// <summary>
        ///     Gets the data model observer.
        /// </summary>
        public static IWireupObserver Observer { get { return __observer; } }
    }
}