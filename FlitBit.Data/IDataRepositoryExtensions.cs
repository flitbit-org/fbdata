using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using FlitBit.Core.Parallel;

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
	}
}
