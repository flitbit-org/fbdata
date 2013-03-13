using System;
using System.Collections.Generic;
using FlitBit.Core;

namespace FlitBit.Data
{
	[Serializable]
	public sealed class DataModelReference<M, IK> : IDataModelReference<M, IK>, IDataModelReferent<M, IK>,
																									IEquatable<DataModelReference<M, IK>>
	{
		static readonly int CHashCodeSeed = typeof(DataModelReference<M, IK>).AssemblyQualifiedName.GetHashCode();
		public static readonly IEqualityComparer<IK> KeyComparer = EqualityComparer<IK>.Default;

		IK _id;
		M _model;
		IK id;

		public DataModelReference() { }

		public DataModelReference(M model)
		{
			_model = model;
			if (_model != null)
			{
				_id = (IK) DataModel<M>.IdentityKey.UntypedKey(_model);
			}
		}

		public DataModelReference(IK id) { _id = id; }

		public bool IsEmpty
		{
			get
			{
				return _model == null
					&& KeyComparer.Equals(_id, default(IK));
			}
		}

		public bool HasModel
		{
			get { return _model != null; }
		}

		public M Model
		{
			get
			{
				if (_model == null && !IsEmpty)
				{
					_model = DataModel<M>.ResolveIdentityKey(_id);
				}
				return _model;
			}
		}

		public IK IdentityKey
		{
			get { return _id; }
		}

		public object Clone() { return (DataModelReference<M, IK>) this.MemberwiseClone(); }

		public void SetIdentityKey(IK id)
		{
			if (!IsEmpty)
			{
				throw new InvalidOperationException("References are write-once.");
			}
			_id = id;
		}

		public void SetReferent(M model)
		{
			if (!IsEmpty)
			{
				throw new InvalidOperationException("References are write-once.");
			}
			_model = model;
			if (_model != null)
			{
				_id = (IK) DataModel<M>.IdentityKey.UntypedKey(_model);
			}
		}

		public bool Equals(DataModelReference<M, IK> other)
		{
			var res = other != null && KeyComparer.Equals(_id, other._id);
			if (res)
			{
				if (_model != null)
				{
					res = _model.Equals(other._model);
				}
				else
				{
					res = other._model == null;
				}
			}
			return res;
		}

		public override bool Equals(object obj)
		{
			return obj is DataModelReference<M, IK>
				&& Equals((DataModelReference<M, IK>) obj);
		}

		public override int GetHashCode()
		{
			var prime = Constants.NotSoRandomPrime;
			var res = CHashCodeSeed*prime;
			if (_id != null)
			{
				res ^= _id.GetHashCode()*prime;
			}
			if (_model != null)
			{
				res ^= _model.GetHashCode()*prime;
			}
			return res;
		}
	}
}