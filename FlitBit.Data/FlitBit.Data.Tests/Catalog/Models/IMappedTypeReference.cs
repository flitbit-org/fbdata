using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlitBit.Data.Catalog;

namespace FlitBit.Data.Tests.Catalog.Models
{
	[Serializable]
	public class IMappedTypeReference: IDataModelReference<IMappedType, int>, IDataModelReferent<IMappedType, int>
	{
		int _id;
		IMappedType _model;

		public IMappedTypeReference()
		{
		}

		public bool IsEmpty { get { return _model == null && _id == 0; } }

		public bool HasModel { get { return _model != null; } }

		public IMappedType Model { get { return _model; } }

		public int IdentityKey { get { return _id; } }
				
		public object Clone()
		{						 
			var res = new IMappedTypeReference();
			if (!IsEmpty)
			{
				if (HasModel) res.SetReferent(_model);
				else res.SetIdentityKey(_id);
			}
			
			return res;
		}

		public void SetIdentityKey(int id)
		{
			if (!IsEmpty) throw new InvalidOperationException("References are write-once.");
			_id = id;
		}

		public void SetReferent(IMappedType model)
		{
			if (!IsEmpty) throw new InvalidOperationException("References are write-once.");
			_model = model;
			if (model != null)
			{
				_id = model.ID;
			}
		}
	}
}
