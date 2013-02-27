#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Collections.Generic;
using System.Linq;
using FlitBit.Core.Parallel;
using System.Diagnostics.Contracts;
using System;

namespace FlitBit.Data
{
	/// <summary>
	/// Basic repository interface for performing CRUD over model type TModel.
	/// </summary>
	/// <typeparam name="TModel">model type TModel</typeparam>
	/// <typeparam name="Id">model's identity type Id</typeparam>
	[ContractClass(typeof(CodeContracts.ContractForIDataRepository<,>))]
	public interface IDataRepository<TModel, Id>
	{
		Id GetIdentity(TModel model);

		TModel Create(IDbContext context, TModel model);
		TModel Read(IDbContext context, Id id);
		TModel Update(IDbContext context, TModel model);
		bool Delete(IDbContext context, Id id);
		
		IEnumerable<TModel> All(IDbContext context);

		IEnumerable<TModel> ReadMatch<TMatch>(IDbContext context, TMatch match);
		int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update);
		int DeleteMatch<TMatch>(IDbContext context, TMatch match);
				
		void Create(IDbContext context, TModel model, Continuation<TModel> continuation);
		void Read(IDbContext context, Id id, Continuation<TModel> continuation);
		void Update(IDbContext context, TModel model, Continuation<TModel> continuation);
		void Delete(IDbContext context, Id id, Continuation<bool> continuation);				 

		void All(IDbContext context, Continuation<IEnumerable<TModel>> continuation);
		void ReadMatch<TMatch>(IDbContext context, TMatch match, Continuation<IEnumerable<TModel>> continuation);
		void UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update, Continuation<int> continuation);
		void DeleteMatch<TMatch>(IDbContext context, TMatch match, Continuation<int> continuation);

		IQueryable<TModel> Query();
	}

	namespace CodeContracts
	{
		/// <summary>
		/// CodeContracts Class for IDataRepository&lt;,>
		/// </summary>
		[ContractClassFor(typeof(IDataRepository<,>))]
		internal abstract class ContractForIDataRepository<M, I> : IDataRepository<M, I>
		{									 
			public I GetIdentity(M model)
			{
				Contract.Requires<ArgumentNullException>(model != null);

				throw new NotImplementedException();
			}

			public M Create(IDbContext context, M model)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(model != null);
				Contract.Ensures(Contract.Result<M>() != null);

				throw new NotImplementedException();
			}

			public M Read(IDbContext context, I id)
			{
				Contract.Requires<ArgumentNullException>(context != null);

				throw new NotImplementedException();
			}

			public M Update(IDbContext context, M model)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(model != null);
				Contract.Ensures(Contract.Result<M>() != null);

				throw new NotImplementedException();
			}

			public bool Delete(IDbContext context, I id)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				throw new NotImplementedException();
			}

			public IEnumerable<M> All(IDbContext context)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Ensures(Contract.Result<IEnumerable<M>>() != null);

				throw new NotImplementedException();
			}

			public IEnumerable<M> ReadMatch<TMatch>(IDbContext context, TMatch match)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Ensures(Contract.Result<IEnumerable<M>>() != null);
				throw new NotImplementedException();
			}

			public int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Requires<ArgumentNullException>(update != null);
				throw new NotImplementedException();
			}

			public int DeleteMatch<TMatch>(IDbContext context, TMatch match)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				throw new NotImplementedException();
			}

			public void Create(IDbContext context, M model, Continuation<M> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(model != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public void Read(IDbContext context, I id, Continuation<M> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public void Update(IDbContext context, M model, Continuation<M> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(model != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public void Delete(IDbContext context, I id, Continuation<bool> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public void All(IDbContext context, Continuation<IEnumerable<M>> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public void ReadMatch<TMatch>(IDbContext context, TMatch match, Continuation<IEnumerable<M>> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public void UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update, Continuation<int> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Requires<ArgumentNullException>(update != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public void DeleteMatch<TMatch>(IDbContext context, TMatch match, Continuation<int> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				throw new NotImplementedException();
			}

			public IQueryable<M> Query()
			{
				Contract.Ensures(Contract.Result<IQueryable<M>>() != null);

				throw new NotImplementedException();
			}
		}
	}
		
}
