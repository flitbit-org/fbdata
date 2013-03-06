using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	public static class IDataRepositoryExtensions
	{
		
		public static TModel Create<TModel, Id>(this IDataRepository<TModel, Id> repo, TModel model)
		{
			Contract.Requires<ArgumentNullException>(repo != null);
			Contract.Requires<ArgumentNullException>(model != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.Create(context, model);
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
		
		public static TModel Read<TModel, Id>(this IDataRepository<TModel, Id> repo, Id id)
		{
			Contract.Requires<ArgumentNullException>(repo != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.Read(context, id);
			}
		}

		public static IEnumerable<TModel> All<TModel, Id>(this IDataRepository<TModel, Id> repo, QueryBehavior behavior)
		{
			Contract.Requires<ArgumentNullException>(repo != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.All(context, behavior);
			}
		}

		public static IEnumerable<TModel> All<TModel, Id>(this IDataRepository<TModel, Id> repo, IDbContext context)
		{
			Contract.Requires<ArgumentNullException>(repo != null);
			Contract.Requires<ArgumentNullException>(context != null);

			return repo.All(context, QueryBehavior.Default);			
		}

		public static IEnumerable<TModel> ReadMatch<TModel, Id, TMatch>(this IDataRepository<TModel, Id> repo, QueryBehavior behavior, TMatch match)
				where TMatch : class
		{
			Contract.Requires<ArgumentNullException>(repo != null);
			Contract.Requires<ArgumentNullException>(match != null);

			using (var context = DbContext.SharedOrNewContext())
			{
				return repo.ReadMatch(context, behavior, match);
			}
		}

		public static IEnumerable<TModel> ReadMatch<TModel, Id, TMatch>(this IDataRepository<TModel, Id> repo, IDbContext context, TMatch match)
				where TMatch : class
		{
			Contract.Requires<ArgumentNullException>(repo != null);
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(match != null);

			return repo.ReadMatch(context, QueryBehavior.Default, match);
		}
	}
}
