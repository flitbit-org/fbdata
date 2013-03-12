using System;
using System.Collections.Generic;
using FlitBit.Core;

namespace FlitBit.Data
{
	public abstract class DataModelReferenceFactory<M, IK> : IDataModelReferenceFactory<M>
	{			 
		public IDataModelReference<M> MakeFromReferent(M model)
		{
			return new DataModelReference<M, IK>(model);
		}

		public IDataModelReference<M, IK> MakeFromReferentID<IK>(IK id)
		{
			return new DataModelReference<M, IK>(id);
		}
	}
}
