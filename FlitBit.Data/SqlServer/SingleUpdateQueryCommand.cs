#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data.SqlClient;
using FlitBit.Core.Collections;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	/// Abstract base; command intended to store a single instance.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	public abstract class SingleUpdateQueryCommand<TDataModel, TImpl> : IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>
		where TImpl : class, TDataModel, IDataModel, new()
	{
		readonly DynamicSql _sql;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="offsets"></param>
		protected SingleUpdateQueryCommand(DynamicSql sql, int[] offsets)
		{
			_sql = sql;
			_offsets = offsets;
		} 
		
		/// <summary>
		/// Executes the command with the specified parameters. 
		/// </summary>
		/// <param name="cx"></param>
		/// <param name="cn"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TDataModel param)
		{
			var impl = param as TImpl;
			if (impl == null)
			{	
				throw new InvalidOperationException("You must transform the instance to the mapped concrete type before saving.");
			}

			BitVector dirty = impl.GetDirtyFlags();
			if (dirty.TrueFlagCount == 0)
			{
				return param;
			} 
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand())
			{
				BindCommand(cmd, _sql, impl, dirty, _offsets);
				using (var reader = cmd.ExecuteReader())
				{
					cx.IncrementQueryCounter();
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, _offsets);
					}
					if (reader.Read()) throw new DuplicateObjectException();
				}
			}
			return res;
		}

		/// <summary>
		/// Implemented by specialized classes to bind the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="sql"></param>
		/// <param name="model"></param>
		/// <param name="dirty"></param>
		/// <param name="offsets"></param>
		protected abstract void BindCommand(SqlCommand cmd, DynamicSql sql, TImpl model, BitVector dirty, int[] offsets);
	}
}
