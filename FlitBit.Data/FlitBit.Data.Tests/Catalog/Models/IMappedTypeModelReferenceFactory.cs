using FlitBit.Data.Catalog;
using System;

namespace FlitBit.Data.Tests.Catalog.Models
{
	
	public class IMappedTypeModelReferenceFactory : IDataModelReferenceFactory<IMappedType>
	{
		public IDataModelReference<IMappedType> MakeFromReferent(IMappedType model)
		{
			var res = new IMappedTypeReference();
			if (model != null)
			{
				res.SetReferent(model);
			}
			return res;
		}

		public IDataModelReference<IMappedType, IK> MakeFromReferentID<IK>(IK id)
		{
			if (!typeof(int).IsAssignableFrom(typeof(IK)))
			{
				throw new ArgumentException("Identity key type mismatch");
			}
			var res = new IMappedTypeReference();
			return res.SetIdentityKey(id);
		}
	}
}
