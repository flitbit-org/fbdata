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
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1> ConstructCommandOnConstraints(Constraints constraints)
		{
			var cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2> ConstructCommandOnConstraints(Constraints constraints)
		{
			var cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3> ConstructCommandOnConstraints(Constraints constraints)
		{
			var cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4> ConstructCommandOnConstraints(Constraints constraints)
		{
			Type cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> ConstructCommandOnConstraints(Constraints constraints)
		{
			Type cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> ConstructCommandOnConstraints(Constraints constraints)
		{
			Type cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> ConstructCommandOnConstraints(Constraints constraints)
		{
			Type cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> ConstructCommandOnConstraints(Constraints constraints)
		{
			Type cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
	/// <summary>
	/// Data model command builder for SQL Server.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	/// <typeparam name="TParam9"></typeparam>
	public class SqlDataModelCommandBuilder<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> :
		DataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
		where TImpl : class, IDataModel, TDataModel, new()
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="sqlWriter"></param>
		public SqlDataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
			: base(queryKey, sqlWriter)
		{
		}

		/// <summary>
		/// Builds a query command with the specified constraints.
		/// </summary>
		/// <param name="constraints"></param>
		/// <returns></returns>
		protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> ConstructCommandOnConstraints(Constraints constraints)
		{
			Type cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(Mapping<TDataModel>.Instance, QueryKey, constraints);
			return
				(IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>)
					Activator.CreateInstance(cmd, constraints.Writer.Text, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
		}
	}
}
