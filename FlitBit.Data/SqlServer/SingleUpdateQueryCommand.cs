using System;
using System.Data;
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
		readonly string _commandText;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="commandText"></param>
		/// <param name="offsets"></param>
		protected SingleUpdateQueryCommand(string commandText, int[] offsets)
		{
			_commandText = commandText;
			this._offsets = offsets;
		} 
		
		/// <summary>
		/// Executes the command with the specified parameters. 
		/// </summary>
		/// <param name="cx"></param>
		/// <param name="cn"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TDataModel model)
		{
			var impl = model as TImpl;
			if (impl == null)
			{	
				throw new InvalidOperationException("You must transform the instance to the mapped concrete type before saving.");
			}

			BitVector dirty = impl.GetDirtyFlags();
			if (dirty.TrueFlagCount == 0)
			{
				return model;
			} 
			TImpl res = default(TImpl);
			using (var cmd = cn.CreateCommand(_commandText, CommandType.Text))
			{
				BindCommand((SqlCommand)cmd, impl, dirty, _offsets);
				cmd.Prepare();
				using (var reader = cmd.ExecuteReader())
				{
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
		/// <param name="model"></param>
		/// <param name="dirty"></param>
		/// <param name="offsets"></param>
		protected abstract void BindCommand(SqlCommand cmd, TImpl model, BitVector dirty, int[] offsets);
	}
}
