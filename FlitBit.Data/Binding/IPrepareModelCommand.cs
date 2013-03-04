using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace FlitBit.Data.Binding
{
	public interface IPrepareModelCommand<in TModel, Id, out THandback, in TDbConnection, in TDbCommand, in TDbDataReader>
		where TDbConnection : DbConnection
		where TDbCommand : DbCommand
		where TDbDataReader : DbDataReader
	{
		THandback PrepareCommand(TDbConnection cn, TDbCommand cmd, Id id);
		THandback PrepareCommand(TDbConnection cn, TDbCommand cmd, TModel model);
	}
}
