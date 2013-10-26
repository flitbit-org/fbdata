using System;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TCriteria> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TCriteria>
	{
		public SqlDataModelCommandBuilder(DataModelSqlWriter<TDataModel> sqlWriter)
			: base(sqlWriter)
		{
		}

		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TCriteria> ConstructCommandOnConstraints(Constraints constraints)
		{
			throw new NotImplementedException();
		}
	}

}
