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
		void Create(IDbContext context, TModel model, Continuation<TModel> continuation);
		void Read(IDbContext context, Id id, Continuation<TModel> continuation);
		void Update(IDbContext context, TModel model, Continuation<TModel> continuation);
		void Delete(IDbContext context, Id id, Continuation<bool> continuation);

		void All(IDbContext context, Continuation<IEnumerable<TModel>> continuation);
		void Match<TMatch>(IDbContext context, TMatch match, Continuation<IEnumerable<TModel>> continuation);
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

				throw new System.NotImplementedException();
			}

			public void Create(IDbContext context, M model, Continuation<M> continuation)
			{
				throw new System.NotImplementedException();
			}

			public void Read(IDbContext context, I id, Continuation<M> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(continuation != null);

				throw new System.NotImplementedException();
			}

			public void Update(IDbContext context, M model, Continuation<M> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(model != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				
				throw new System.NotImplementedException();
			}

			public void Delete(IDbContext context, I id, Continuation<bool> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				
				throw new System.NotImplementedException();
			}

			public void All(IDbContext context, Continuation<IEnumerable<M>> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				
				throw new System.NotImplementedException();
			}

			public void Match<TMatch>(IDbContext context, TMatch match, Continuation<IEnumerable<M>> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Requires<ArgumentNullException>(continuation != null);
				
				throw new System.NotImplementedException();
			}

			public IQueryable<M> Query()
			{
				Contract.Ensures(Contract.Result<IQueryable<M>>() != null);

				throw new System.NotImplementedException();
			}
		}
	}
		
}
