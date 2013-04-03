#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
		/// Queries all models of the type.
		/// </summary>
		/// <param name="context">a db context</param>
		/// <param name="behavior">the query's behaviors.</param>
		/// <returns>a query result</returns>
		IDataModelQueryResult<TModel> All(IDbContext context, QueryBehavior behavior);

		/// <summary>
		/// Saves/creates a new model on the underlying data store from the model provided.
		/// </summary>
		/// <param name="context">a db context</param>
		/// <param name="model">the new model</param>
		/// <returns>the model, updated with any calculated values from the underlying data store.</returns>
		TModel Create(IDbContext context, TModel model);

		/// <summary>
		/// Deletes the model associated with the identity key provided.
		/// </summary>
		/// <param name="context">a db context</param>
		/// <param name="id">identity key of the model to delete</param>
		/// <returns><em>true</em> if successful; otherwise <em>false</em></returns>
		bool Delete(IDbContext context, TIdentityKey id);

		/// <summary>
		/// Deletes all models of the type matching a match specification.
		/// </summary>
		/// <typeparam name="TMatch">the match specification's type</typeparam>
		/// <param name="context">a db context</param>
		/// <param name="match">the match specification</param>
		/// <returns>the number of models affected</returns>
		int DeleteMatch<TMatch>(IDbContext context, TMatch match)
			where TMatch : class;

		/// <summary>
		/// Gets a model's identity key.
		/// </summary>
		/// <param name="model">the model</param>
		/// <returns>the model's identity key</returns>
		TIdentityKey GetIdentity(TModel model);

		/// <summary>
		/// Accesses an IQueryable capable of performing ad-hoc LINQ queries against the underlying type.
		/// </summary>
		/// <returns></returns>
		IQueryable<TModel> Query();

		/// <summary>
		/// Reads/retreives the model associated with the identity key provided.
		/// </summary>
		/// <param name="context">a db context</param>
		/// <param name="id">an identity key</param>
		/// <returns>the model identified by the provided key</returns>
		TModel Read(IDbContext context, TIdentityKey id);

		/// <summary>
		/// Queries/retrieves the models matching the match specification provided.
		/// </summary>
		/// <typeparam name="TMatch">a match specification type</typeparam>
		/// <param name="context">a db context</param>
		/// <param name="behavior">the query's behavior</param>
		/// <param name="match">a match specification</param>
		/// <returns>a query result</returns>
		IDataModelQueryResult<TModel> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match)
			where TMatch : class;

		/// <summary>
		/// Stores/updates an existing model on the underlying data store from the model provided.
		/// </summary>
		/// <param name="context">a db context</param>
		/// <param name="model">the model</param>
		/// <returns>the model, updated with any calculated values from the underlying data store.</returns>
		TModel Update(IDbContext context, TModel model);

		/// <summary>
		/// Updates models matching the match specification provided, according to the update specification provided.
		/// </summary>
		/// <typeparam name="TMatch">a match specification type</typeparam>
		/// <typeparam name="TUpdate">an updated specification type</typeparam>
		/// <param name="context">a db context</param>
		/// <param name="match">a match specification</param>
		/// <param name="update">an update specification</param>
		/// <returns>the number of models affected.</returns>
		int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)
			where TMatch : class
			where TUpdate : class;
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
				Contract.Requires<ArgumentNullException>(model != null);
				Contract.Ensures(Contract.Result<TModel>() != null);

				throw new NotImplementedException();
			}

			public TModel Read(IDbContext context, TIdentityKey id)
			{
				Contract.Requires<ArgumentNullException>(context != null);

				throw new NotImplementedException();
			}

			public TModel Update(IDbContext context, TModel model)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(model != null);
				Contract.Ensures(Contract.Result<TModel>() != null);

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
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Ensures(Contract.Result<IDataModelQueryResult<TModel>>() != null);

				throw new NotImplementedException();
			}

			public IDataModelQueryResult<TModel> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match)
				where TMatch : class
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Ensures(Contract.Result<IDataModelQueryResult<TModel>>() != null);
				throw new NotImplementedException();
			}

			public int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update) 
				where TMatch : class
				where TUpdate : class
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Requires<ArgumentNullException>(update != null);
				throw new NotImplementedException();
			}

			public int DeleteMatch<TMatch>(IDbContext context, TMatch match) where TMatch : class
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				throw new NotImplementedException();
			}

			public IQueryable<TModel> Query()
			{
				Contract.Ensures(Contract.Result<IQueryable<TModel>>() != null);

				throw new NotImplementedException();
			}

			#endregion
		}
	}
}