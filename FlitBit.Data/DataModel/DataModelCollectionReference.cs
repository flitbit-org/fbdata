#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;

namespace FlitBit.Data.DataModel
{
  /// <summary>
  ///   Lazy-fetch wrapper for referenced collections (parent child relationships and such).
  /// </summary>
  /// <typeparam name="TDataModel">referent type TDataModel</typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  public sealed class DataModelCollectionReference<TDataModel, TIdentityKey, TDbConnection, TParam>
    : IEquatable<DataModelCollectionReference<TDataModel, TIdentityKey, TDbConnection, TParam>>
    where TDbConnection : DbConnection
  {
// ReSharper disable once StaticFieldInGenericType
    static readonly int CHashCodeSeed =
      typeof(DataModelCollectionReference<TDataModel, TIdentityKey, TDbConnection, TParam>).AssemblyQualifiedName
                                                                                           .GetHashCode();

    readonly IDataModelQueryCommand<TDataModel, TDbConnection, TParam> _command;
    ObservableCollection<TDataModel> _collection;
    bool _resolved;
    bool _isFaulted;
    Exception _exception;
    readonly object _sync = new object();
    NotifyCollectionChangedEventHandler _referrerNotify;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="name">Name of the reference.</param>
    /// <param name="cmd">the command that performs the query</param>
    /// <param name="referrerNotify">A callback to notify the referrer that the collection has been resolved.</param>
    /// <param name="param">parameter used to select the referenced items</param>
    public DataModelCollectionReference(string name, IDataModelQueryCommand<TDataModel, TDbConnection, TParam> cmd,
      NotifyCollectionChangedEventHandler referrerNotify, TParam param)
    {
      Contract.Requires<ArgumentNullException>(name != null);
      Contract.Requires<ArgumentNullException>(name.Length > 0);
      Contract.Requires<ArgumentNullException>(cmd != null);

      ReferenceName = name;
      _command = cmd;
      _referrerNotify = referrerNotify;
      Param = param;
    }

    public string ReferenceName { get; private set; }

    /// <summary>
    ///   The first parameter used to resolve the collection.
    /// </summary>
    public TParam Param { get; private set; }

    /// <summary>
    ///   Gets the referenced collection; resovling it if necessary.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DataModelReferenceException"></exception>
    public ObservableCollection<TDataModel> GetCollection()
    {
      Contract.Ensures(Contract.Result<ObservableCollection<TDataModel>>() != null);
      if (!_resolved)
      {
        lock (_sync)
        {
          if (!_resolved)
          {
            var repo =
              (IDataModelRepository<TDataModel, TIdentityKey, TDbConnection>)
              DataModel<TDataModel>.GetRepository<TIdentityKey>();
            IDataModelQueryResult<TDataModel> res;
            using (var cx = DbContext.SharedOrNewContext())
            {
              res = repo.ExecuteMany(_command, cx, QueryBehavior.Default, Param);
            }
            if (res.Succeeded)
            {
              _collection = new ObservableCollection<TDataModel>(res.Results);
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
          String.Concat("Unable to resolve reference: ", ReferenceName, "(", typeof(TParam).GetReadableSimpleName(),
            ") -> ",
            typeof(TDataModel).GetReadableSimpleName(), "."), _exception);
      }
      return _collection;
    }

    /// <summary>
    ///   Determines if this reference is equal to another.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(DataModelCollectionReference<TDataModel, TIdentityKey, TDbConnection, TParam> other)
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
    ///   Determines if this object is equal to another.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      var other = obj as DataModelCollectionReference<TDataModel, TIdentityKey, TDbConnection, TParam>;
      return other != null && Equals(other);
    }

    /// <summary>
    ///   Calculates the hashcode based on the collection's content (variant).
    /// </summary>
    /// <returns>
    ///   A hash code for the current <see cref="T:System.Object" />.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override int GetHashCode()
    {
      const int prime = Constants.NotSoRandomPrime;

      var code = CHashCodeSeed * prime;
      code ^= ReferenceName.GetHashCode() * prime;
// ReSharper disable once NonReadonlyFieldInGetHashCode
      code ^= ((null != _collection) ? _collection.CalculateCombinedHashcode(code) : 0) * prime;
      code ^= (typeof(TParam).IsValueType || ((object)Param != null) ? Param.GetHashCode() : 0) * prime;
      return code;
    }

    /// <summary>
    ///   Clones the reference for a new referrer.
    /// </summary>
    /// <param name="referrerNotify"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public DataModelCollectionReference<TDataModel, TIdentityKey, TDbConnection, TParam> Clone(
      NotifyCollectionChangedEventHandler referrerNotify)
    {
      var clone = (DataModelCollectionReference<TDataModel, TIdentityKey, TDbConnection, TParam>)MemberwiseClone();
      clone._referrerNotify = referrerNotify;
      if (clone._collection != null)
      {
        clone._collection = new ObservableCollection<TDataModel>(clone._collection);
        if (referrerNotify != null)
        {
          clone._collection.CollectionChanged += referrerNotify;
        }
      }
      return clone;
    }
  }
}