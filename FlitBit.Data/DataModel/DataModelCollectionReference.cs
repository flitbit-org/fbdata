using FlitBit.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	/// Lazy-fetch wrapper for referenced collections (parent child relationships and such).
	/// </summary>
	/// <typeparam name="TReferent">referent type TReferent</typeparam>
	/// <typeparam name="TParam"></typeparam>
	public sealed class DataModelCollectionReference<TReferent, TParam> 
		: IEquatable<DataModelCollectionReference<TReferent, TParam>>
	{
// ReSharper disable once StaticFieldInGenericType
		static readonly int CHashCodeSeed = typeof(DataModelCollectionReference<TReferent, TParam>).AssemblyQualifiedName.GetHashCode();

		private ObservableCollection<TReferent> _collection;
		private bool _resolved;
		private bool _isFaulted;
		private Exception _exception;
		private readonly object _sync = new object();
		private NotifyCollectionChangedEventHandler _referrerNotify;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="name">Name of the reference.</param>
		/// <param name="referrerNotify">A callback to notify the referrer that the colleciton has been resolved.</param>
		/// <param name="param">parameter used to select the referenced items</param>
		public DataModelCollectionReference(string name, NotifyCollectionChangedEventHandler referrerNotify, TParam param)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);

			ReferenceName = name;
			_referrerNotify = referrerNotify;
			Param = param;
		}

		/// <summary>
		/// The resolver.
		/// </summary>
		public Func<TParam, IDataModelQueryResult<TReferent>> Resolver { get; set; }

		/// <summary>
		/// The first parameter used to resolve the collection.
		/// </summary>
		public TParam Param { get; private set; }

		/// <summary>
		/// The reference's name from the perspective of the referrer.
		/// </summary>
		public string ReferenceName { get; private set; }

		/// <summary>
		/// Gets the referenced collection; resovling it if necessary.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="DataModelReferenceException"></exception>
		public ObservableCollection<TReferent> GetCollection()
		{
			Contract.Requires<InvalidOperationException>(Resolver != null);
			Contract.Ensures(Contract.Result<ObservableCollection<TReferent>>() != null);
			if (!_resolved)
			{
				lock (_sync)
				{
					if (_resolved)
					{
						var res = Resolver(Param);
						if (res.Succeeded)
						{
							_collection = new ObservableCollection<TReferent>(res.Results);
							if (_referrerNotify != null)
							{
								_collection.CollectionChanged += _referrerNotify;
							}
						}
						else
						{
							_isFaulted = true;
							_exception = res.Exception;
						}
						_resolved = true;
					}
				}
			}
			if (_isFaulted)
			{
				throw new DataModelReferenceException(
					String.Concat("Unable to resolve reference: ", ReferenceName, "(", typeof(TParam).GetReadableSimpleName(), ") -> ", 
					typeof (TReferent).GetReadableSimpleName(), "."), _exception);
			}
			return _collection;
		}

		/// <summary>
		/// Determines if this reference is equal to another.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(DataModelCollectionReference<TReferent, TParam> other)
		{
			var result = other != null
									 && String.Equals(ReferenceName, other.ReferenceName)
									 && EqualityComparer<TParam>.Default.Equals(Param, other.Param)
									 ;

			if (result && _collection != null)
			{
				if (other._collection != null)
				{
					return _collection.Intersect(other._collection).Count() == _collection.Count();
				}
				return false;
			}
			return result && other._collection == null;
		}

		/// <summary>
		/// Determines if this object is equal to another.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var other = obj as DataModelCollectionReference<TReferent, TParam>;
			return other != null && Equals(other);
		}

		/// <summary>
		/// Calculates the hashcode based on the collection's content (variant).
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			const int prime = Constants.NotSoRandomPrime;

			int code = CHashCodeSeed * prime;
			code ^= ReferenceName.GetHashCode() * prime;
// ReSharper disable once NonReadonlyFieldInGetHashCode
			code ^= ((null != _collection) ? _collection.CalculateCombinedHashcode(code) : 0) * prime;
			code ^= (typeof(TParam).IsValueType || ((object)Param != null) ? Param.GetHashCode() : 0) * prime;
			return code;
		}

		/// <summary>
		/// Clones the reference for a new referrer.
		/// </summary>
		/// <param name="referrerNotify"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public DataModelCollectionReference<TReferent, TParam> Clone(NotifyCollectionChangedEventHandler referrerNotify)
		{
			var clone = (DataModelCollectionReference<TReferent, TParam>)MemberwiseClone();
			clone._referrerNotify = referrerNotify;
			if (clone._collection != null)
			{
				clone._collection = new ObservableCollection<TReferent>(clone._collection);
				if (referrerNotify != null)
				{
					clone._collection.CollectionChanged += referrerNotify;
				}
			}
			return clone;
		}
	}

}
