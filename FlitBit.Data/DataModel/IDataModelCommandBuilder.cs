using System;
using System.Linq.Expressions;

namespace FlitBit.Data.DataModel
{
	public interface IDataModelCommandBuilder<TDataModel, in TDbConnection, TCriteria>
	{
		IDataModelQueryCommand<TDataModel, TDbConnection, TCriteria> Where(
			Expression<Func<TDataModel, TCriteria, bool>> expression);
	}
}