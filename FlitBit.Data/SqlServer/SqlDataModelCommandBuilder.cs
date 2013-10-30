using System;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam>
		where TImpl: class, IDataModel, TDataModel, new()
	{

		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam> ConstructCommandOnConstraints(Constraints constraints)
		{
			Type cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}

}
