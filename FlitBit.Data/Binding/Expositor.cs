using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace FlitBit.Data.Binding
{
	public abstract class Expositor<TModel, Id, TModelImpl, TDbConnection, TDbCommand, TDbDataReader, THandback>
		: IPrepareModelCommand<TModel, Id, THandback, TDbConnection, TDbCommand, TDbDataReader>
		, ILoadModel<TModel, THandback, TDbConnection, TDbCommand, TDbDataReader>
		where TModelImpl : class, TModel, new()
		where TDbConnection : DbConnection
		where TDbCommand : DbCommand
		where TDbDataReader : DbDataReader
	{
		protected abstract THandback PrepareCommand(TDbConnection cn, TDbCommand cmd, TModelImpl model);

		public abstract THandback PrepareCommand(TDbConnection cn, TDbCommand cmd, Id id);

		public abstract TModel LoadSingle(TDbConnection cn, TDbCommand cmd, TDbDataReader reader, THandback handback);

		public abstract IEnumerable<TModel> LoadMany(TDbConnection cn, TDbCommand cmd, TDbDataReader reader, THandback handback);

		THandback IPrepareModelCommand<TModel, Id, THandback, TDbConnection, TDbCommand, TDbDataReader>.PrepareCommand(TDbConnection cn, TDbCommand cmd, TModel model)
		{
			return PrepareCommand(cn, cmd, (TModelImpl)model);
		}
	}
}
