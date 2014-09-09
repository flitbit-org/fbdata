#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.Data.DataModel;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using FlitBit.Wireup.Recording;

namespace FlitBit.Data.Meta
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class DataModelAttribute : WireupTaskAttribute
    {
        public DataModelAttribute(Type prepareDataMappingType)
            : base(WireupPhase.Tasks) { PrepareDataMappingType = prepareDataMappingType; }

        public Type PrepareDataMappingType { get; private set; }

        protected override void PerformTask(IWireupCoordinator coordinator, WireupContext context) { }
    }

    public abstract class PrepareDataMapping
    {
        public abstract void UntypedPrepareMapping(IMapping mapping);
    }

    public abstract class PrepareDataMapping<TModel> : PrepareDataMapping
    {
        protected abstract void PrepareMapping(IMapping<TModel> mapping);

        public override void UntypedPrepareMapping(IMapping mapping) { PrepareMapping((IMapping<TModel>)mapping); }
    }
}