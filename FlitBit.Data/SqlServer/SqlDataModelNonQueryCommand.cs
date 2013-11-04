using FlitBit.Data.DataModel;
using System.Data.SqlClient;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	/// Abstract base; command intended to produce a single instance result.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	public abstract class SqlDataModelNonQueryCommand<TDataModel, TParam> : IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam>
	{
		readonly DynamicSql _sql;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="sql">Initial command text.</param>
		/// <param name="offsets">column offsets within the results returned by the command</param>
		protected SqlDataModelNonQueryCommand(DynamicSql sql, int[] offsets)
		{
			_sql = sql;
			_offsets = offsets;
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param)
		{
			using (var cmd = cn.CreateCommand(_sql.Text, _sql.CommandType))
			{
				BindCommand((SqlCommand)cmd, param, _offsets);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="param"></param>
		/// <param name="offsets"></param>
		protected abstract void BindCommand(SqlCommand cmd, TParam param, int[] offsets);
	}
}
