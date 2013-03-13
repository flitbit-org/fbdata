#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Data.CodeContracts;

namespace FlitBit.Data
{
	/// <summary>
	///   Basic repository interface for performing CRUD over model type TModel.
	/// </summary>
	/// <typeparam name="M">model type M</typeparam>
	/// <typeparam name="IK">identity key type IK</typeparam>
	[ContractClass(typeof(ContractForIDataRepository<,>))]
	public interface IDataRepository<M, IK>
	{
		IEnumerable<M> All(IDbContext context, QueryBehavior behavior);
		M Create(IDbContext context, M model);
		bool Delete(IDbContext context, IK id);

		int DeleteMatch<TMatch>(IDbContext context, TMatch match)
			where TMatch : class;

		IK GetIdentity(M model);

		IQueryable<M> Query();
		M Read(IDbContext context, IK id);

		IEnumerable<M> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match)
			where TMatch : class;

		M Update(IDbContext context, M model);

		int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)
			where TMatch : class;
	}

	namespace CodeContracts
	{
		/// <summary>
		///   CodeContracts Class for IDataRepository&lt;,>
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

			public IEnumerable<M> All(IDbContext context, QueryBehavior behavior)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Ensures(Contract.Result<IEnumerable<M>>() != null);

				throw new NotImplementedException();
			}

			public IEnumerable<M> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match)
				where TMatch : class
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<ArgumentNullException>(match != null);
				Contract.Ensures(Contract.Result<IEnumerable<M>>() != null);
				throw new NotImplementedException();
			}

			public int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update) where TMatch : class
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

			public IQueryable<M> Query()
			{
				Contract.Ensures(Contract.Result<IQueryable<M>>() != null);

				throw new NotImplementedException();
			}
		}
	}
}