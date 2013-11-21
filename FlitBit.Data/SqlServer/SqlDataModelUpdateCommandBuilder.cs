using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
    public class SqlDataModelUpdateCommandBuilder<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> 
        : DataModelUpdateCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
    {
	    public SqlDataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter) : base(binder, queryKey, sqlWriter)
	    {
	    }

	    protected override IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> ConstructCommandOnConstraints(Constraints constraints)
	    {
				Type cmd = OneClassOneTableEmitter.MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(Mapping, QueryKey, constraints);
				return
					(IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>)
						Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteUpdate(constraints), Writer.ColumnOffsets);
	    }
    }
}
