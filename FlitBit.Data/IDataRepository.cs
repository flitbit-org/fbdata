#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.Data.CodeContracts;
using FlitBit.Data.DataModel;

namespace FlitBit.Data
{
  /// <summary>
  ///   Basic repository interface for performing CRUD over model type TModel.
  /// </summary>
  /// <typeparam name="TModel">model type M</typeparam>
  /// <typeparam name="TIdentityKey">identity key type IK</typeparam>
  [ContractClass(typeof(ContractForIDataRepository<,>))]
  public interface IDataRepository<TModel, TIdentityKey>
  {
    /// <summary>
    ///   Queries all models of the type.
    /// </summary>
    /// <param name="context">a db context</param>
    /// <param name="behavior">the query's behaviors.</param>
    /// <returns>a query result</returns>
    IDataModelQueryResult<TModel> All(IDbContext context, QueryBehavior behavior);

    /// <summary>
    ///   Saves/creates a new model on the underlying data store from the model provided.
    /// </summary>
    /// <param name="context">a db context</param>
    /// <param name="model">the new model</param>
    /// <returns>the model, updated with any calculated values from the underlying data store.</returns>
    TModel Create(IDbContext context, TModel model);

    /// <summary>
    ///   Deletes the model associated with the identity key provided.
    /// </summary>
    /// <param name="context">a db context</param>
    /// <param name="id">identity key of the model to delete</param>
    /// <returns><em>true</em> if successful; otherwise <em>false</em></returns>
    bool Delete(IDbContext context, TIdentityKey id);

    /// <summary>
    ///   Gets a model's identity key.
    /// </summary>
    /// <param name="model">the model</param>
    /// <returns>the model's identity key</returns>
    TIdentityKey GetIdentity(TModel model);

    /// <summary>
    ///   Reads/retreives the model associated with the identity key provided.
    /// </summary>
    /// <param name="context">a db context</param>
    /// <param name="id">an identity key</param>
    /// <returns>the model identified by the provided key</returns>
    TModel ReadByIdentity(IDbContext context, TIdentityKey id);

    /// <summary>
    ///   Stores/updates an existing model on the underlying data store from the model provided.
    /// </summary>
    /// <param name="context">a db context</param>
    /// <param name="model">the model</param>
    /// <returns>the model, updated with any calculated values from the underlying data store.</returns>
    TModel Update(IDbContext context, TModel model);
  }

  namespace CodeContracts
  {
    /// <summary>
    ///   CodeContracts Class for IDataRepository&lt;,>
    /// </summary>
    [ContractClassFor(typeof(IDataRepository<,>))]
    internal abstract class ContractForIDataRepository<TModel, TIdentityKey> : IDataRepository<TModel, TIdentityKey>
    {
      #region IDataRepository<M,I> Members

      public TIdentityKey GetIdentity(TModel model)
      {
        Contract.Requires<ArgumentNullException>(model != null);

        throw new NotImplementedException();
      }

      public TModel Create(IDbContext context, TModel model)
      {
        Contract.Requires<ArgumentNullException>(context != null);

        throw new NotImplementedException();
      }

      public TModel ReadByIdentity(IDbContext context, TIdentityKey id)
      {
        Contract.Requires<ArgumentNullException>(context != null);

        throw new NotImplementedException();
      }

      public TModel Update(IDbContext context, TModel model)
      {
        Contract.Requires<ArgumentNullException>(context != null);

        throw new NotImplementedException();
      }

      public bool Delete(IDbContext context, TIdentityKey id)
      {
        Contract.Requires<ArgumentNullException>(context != null);
        throw new NotImplementedException();
      }

      public IDataModelQueryResult<TModel> All(IDbContext context, QueryBehavior behavior)
      {
        Contract.Requires<ArgumentNullException>(context != null);
        Contract.Requires<ArgumentNullException>(behavior != null);
        Contract.Ensures(Contract.Result<IDataModelQueryResult<TModel>>() != null);

        throw new NotImplementedException();
      }

      #endregion
    }
  }
}