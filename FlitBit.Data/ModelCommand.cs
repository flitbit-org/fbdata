using System.Collections.Generic;
using System.Data.Common;

namespace FlitBit.Data
{
	public abstract class ModelCommand<TModel, TKey, TModelImpl, TDbConnection>
		: IModelCommand<TModel, TKey, TDbConnection>
		where TDbConnection : DbConnection
		where TModelImpl : class, TModel, new()
	{
		public abstract int Execute(IDbContext cx, TDbConnection cn, TKey key);

		public abstract TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TKey key);

		public abstract IEnumerable<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior, TKey key);
	}
}