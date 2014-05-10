#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using FlitBit.Core.Parallel;
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
			var res = 0;
			using (var cmd = cn.CreateCommand(_sql.Text, _sql.CommandType))
			{
				BindCommand((SqlCommand)cmd, param, _offsets);
				res = cmd.ExecuteNonQuery();
				cx.IncrementQueryCounter();
			}
			cx.IncrementObjectsAffected(res);
			return res;
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
