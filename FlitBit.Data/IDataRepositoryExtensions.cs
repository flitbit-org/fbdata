using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using FlitBit.Data.DataModel;

namespace FlitBit.Data
{
	public static class IDataRepositoryExtensions
	{
		public static IDataModelQueryResult<TModel> All<TModel, Id>(this IDataRepository<TModel, Id> repo, QueryBehavior behavior)
		{
			Contract.Requires<ArgumentNullException>(repo != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.All(context, behavior);
			}
		}

		public static IDataModelQueryResult<TModel> All<TModel, Id>(this IDataRepository<TModel, Id> repo, IDbContext context)
		{
			Contract.Requires<ArgumentNullException>(repo != null);
			Contract.Requires<ArgumentNullException>(context != null);

			return repo.All(context, QueryBehavior.Default);
		}

		public static TModel Create<TModel, Id>(this IDataRepository<TModel, Id> repo, TModel model)
		{
			Contract.Requires<ArgumentNullException>(repo != null);
			Contract.Requires<ArgumentNullException>(model != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.Create(context, model);
			}
		}

		public static TModel Read<TModel, Id>(this IDataRepository<TModel, Id> repo, Id id)
		{
			Contract.Requires<ArgumentNullException>(repo != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.ReadByIdentity(context, id);
			}
		}

		public static TModel Update<TModel, Id>(this IDataRepository<TModel, Id> repo, TModel model)
		{
			Contract.Requires<ArgumentNullException>(repo != null);
			Contract.Requires<ArgumentNullException>(model != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.Update(context, model);
			}
		}
	}
}