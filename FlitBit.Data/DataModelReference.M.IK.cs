using System;
using System.Collections.Generic;
using FlitBit.Core;
using FlitBit.Data.Meta;

namespace FlitBit.Data
{
	[Serializable]
	public sealed class DataModelReference<TModel, TIdentityKey> : IDataModelReference<TModel, TIdentityKey>, IDataModelReferent<TModel, TIdentityKey>,
																									IEquatable<DataModelReference<TModel, TIdentityKey>>, IEquatable<TModel>
	{
		static readonly int CHashCodeSeed = typeof(DataModelReference<TModel, TIdentityKey>).AssemblyQualifiedName.GetHashCode();

		TModel _model;

		public DataModelReference()
		{}

		public DataModelReference(TModel model)
		{
			_model = model;
			if (HasModel)
			{
				this.IdentityKey = (TIdentityKey) Mapping<TModel>.Instance.IdentityKey.UntypedKey(_model);
			}
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="id">an id</param>
		public DataModelReference(TIdentityKey id)
		{
			this.IdentityKey = id;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(TModel other)
		{
			if (IsEmpty) return EqualityComparer<TModel>.Default.Equals(default(TModel), other);
			if (HasModel) return _model.Equals(other);
			if (HasIdentityKey)
			{
				return Model.Equals(other);
			}
			return false;
		}
		
		public override bool Equals(object obj)
		{
			return obj is DataModelReference<TModel, TIdentityKey>
				&& Equals((DataModelReference<TModel, TIdentityKey>) obj);
		}

		public override int GetHashCode()
		{
			const int prime = Constants.NotSoRandomPrime;
			var res = CHashCodeSeed * prime;
			if (HasIdentityKey)
			{
				res ^= this.IdentityKey.GetHashCode() * prime;
			}
			if (HasModel)
			{
				res ^= _model.GetHashCode() * prime;
			}
			return res;
		}

		#region IDataModelReference<M,IK> Members

		public bool IsEmpty
		{
			get { return !HasIdentityKey && !HasModel; }
		}

		/// <summary>
		/// Indicates whether an exception occurred while resolving the reference.
		/// </summary>
		/// <remarks>
		/// If the reference is faulted the <see cref="IDataModelReference{TModel}.Exception"/> property will contain the
		/// exception raised while trying to resolve the reference.
		/// </remarks>
		public bool IsFaulted { get; private set; }

		/// <summary>
		/// Gets the exception thrown while resolving the reference if one occurred; otherwise null.
		/// </summary>
		public Exception Exception { get; private set; }

		public bool HasModel { get { return !EqualityComparer<TModel>.Default.Equals(default(TModel), _model); ; } }

		public bool HasIdentityKey { get { return !EqualityComparer<TIdentityKey>.Default.Equals(default(TIdentityKey), this.IdentityKey); } }

		public TModel Model
		{
			get
			{
				if (!HasModel && HasModel)
				{
					if (IsFaulted)
					{
						throw new DataModelReferenceException("Unable to resolve reference do to prior fault.", this.Exception);
					}
					try
					{
						_model = DataModel<TModel>.ResolveIdentityKey(this.IdentityKey);
					}
					catch (Exception e)
					{
						this.Exception = e;
						throw new DataModelReferenceException("Unable to resolve reference.", e);
					}
				}
				return _model;
			}
		}

		/// <summary>
		/// Gets the referenced model's key type.
		/// </summary>
		public Type IdentityKeyType { get { return typeof(TIdentityKey); } }

		public TIdentityKey IdentityKey { get; private set; }

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		#endregion

		#region IDataModelReferent<M,IK> Members

		public void SetIdentityKey(TIdentityKey id)
		{
			if (!IsEmpty)
			{
				throw new InvalidOperationException("References are write-once.");
			}
			this.IdentityKey = id;
		}

		public void SetReferent(TModel referent)
		{
			if (!IsEmpty)
			{
				throw new InvalidOperationException("References are write-once.");
			}
			_model = referent;
			if (HasModel)
			{
				this.IdentityKey = (TIdentityKey) Mapping<TModel>.Instance.IdentityKey.UntypedKey(_model);
			}
		}

		#endregion

		#region IEquatable<DataModelReference<M,IK>> Members

		public bool Equals(DataModelReference<TModel, TIdentityKey> other)
		{
			var res = other != null 
				&& EqualityComparer<TIdentityKey>.Default.Equals(this.IdentityKey, other.IdentityKey);
			if (res)
			{
				if (HasModel && other.HasModel)
				{
					res = _model.Equals(other._model);
				}
			}
			return res;
		}

		#endregion

		public bool IdentityEquals(object referentID)
		{
			return EqualityComparer<TIdentityKey>.Default.Equals(IdentityKey, (TIdentityKey)referentID);
		}
	}
}