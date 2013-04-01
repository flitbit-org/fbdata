using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using System.Diagnostics.Contracts;

namespace FlitBit.Data.Meta
{
	[AttributeUsage(AttributeTargets.Interface)]
	public class DataModelAttribute : WireupTaskAttribute
	{
		public DataModelAttribute(Type prepareDataMappingType) : base(WireupPhase.Tasks)
		{
			PrepareDataMappingType = prepareDataMappingType;
		}

		public Type PrepareDataMappingType { get; private set; }

		protected override void PerformTask(Wireup.IWireupCoordinator coordinator)
		{	
		}
	}

	public abstract class PrepareDataMapping
	{
		public abstract void UntypedPrepareMapping(IMapping mapping);
	}

	public abstract class PrepareDataMapping<TModel> : PrepareDataMapping
	{
		protected abstract void PrepareMapping(Mapping<TModel> mapping);

		public override void UntypedPrepareMapping(IMapping mapping)
		{
			PrepareMapping((Mapping<TModel>)mapping);
		}
	}
}
