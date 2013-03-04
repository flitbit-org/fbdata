using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace FlitBit.Data.Binding
{
	public interface ILoadModel<out TModel, in THandback, in TDbConnection, in TDbCommand, in TDbDataReader>
		where TDbConnection : DbConnection
		where TDbCommand : DbCommand
		where TDbDataReader : DbDataReader
	{
		TModel LoadSingle(TDbConnection cn, TDbCommand cmd, TDbDataReader reader, THandback handback);
		IEnumerable<TModel> LoadMany(TDbConnection cn, TDbCommand cmd, TDbDataReader reader, THandback handback);
	}
}
