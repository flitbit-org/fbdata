using System;
using System.Linq.Expressions;

namespace FlitBit.Data.DataModel
{
	public interface IDataModelCommandBuilder<TDataModel, in TDbConnection, TCriteria>
	{
		IDataModelQueryManyCommand<TDataModel, TDbConnection, TCriteria> Where(
			Expression<Func<TDataModel, TCriteria, bool>> expression);
	}
}
