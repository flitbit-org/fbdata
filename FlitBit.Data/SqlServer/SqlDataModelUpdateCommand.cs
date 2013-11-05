using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	///     Basic data model query command for queries with one parameter.
	/// </summary>
	public abstract class SqlDataModelUpdateCommand
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
		{
			UpdateQuery = update;
			Offsets = offsets;
		}

		/// <summary>
		///     The update query.
		/// </summary>
		protected DynamicSql UpdateQuery { get; private set; }

		/// <summary>
		///     An array of offsets.
		/// </summary>
		protected int[] Offsets { get; private set; }
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam> : SqlDataModelUpdateCommand,
			IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param)
		{
			var res = 0;
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param);
				res = cmd.ExecuteNonQuery();
				cx.IncrementQueryCounter();
			}
			return res;
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param);
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1> : SqlDataModelUpdateCommand,
			IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam, TParam1>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1);
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2> : SqlDataModelUpdateCommand,
			IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2);
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3> :
			SqlDataModelUpdateCommand,
			IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2,
				TParam3 param3);
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4> :
			SqlDataModelUpdateCommand,
			IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3,
				TParam4 param4)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2,
				TParam3 param3, TParam4 param4);
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> :
			SqlDataModelUpdateCommand,
			IDataModelNonQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3,
				TParam4 param4, TParam5 param5)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2,
				TParam3 param3, TParam4 param4, TParam5 param5);
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5,
			TParam6> : SqlDataModelUpdateCommand,
					IDataModelNonQueryCommand
							<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3,
				TParam4 param4, TParam5 param5, TParam6 param6)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2,
				TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6);
	}

	/// <summary>
	///     Basic data model update query command.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5,
			TParam6, TParam7> : SqlDataModelUpdateCommand,
					IDataModelNonQueryCommand
							<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3,
				TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2,
				TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7);
	}

	/// <summary>
	///     Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5,
			TParam6, TParam7, TParam8> : SqlDataModelUpdateCommand,
					IDataModelNonQueryCommand
							<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7,
									TParam8>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3,
				TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7,
						param8);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2,
				TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8);
	}

	/// <summary>
	///     Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	/// <typeparam name="TParam9"></typeparam>
	public abstract class SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5,
			TParam6, TParam7, TParam8, TParam9> : SqlDataModelUpdateCommand,
					IDataModelNonQueryCommand
							<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7,
									TParam8, TParam9>
	{
		/// <summary>
		///     Creates a new instance.
		/// </summary>
		protected SqlDataModelUpdateCommand(DynamicSql update, int[] offsets)
			: base(update, offsets)
		{
		}

		public int Execute(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3,
				TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
		{
			cn.EnsureConnectionIsOpen();
			using (DbCommand cmd = cn.CreateCommand(UpdateQuery.Text, CommandType.Text))
			{
				int[] offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7,
						param8, param9);
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		///     Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <param name="param9"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2,
				TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8,
				TParam9 param9);
	}
}