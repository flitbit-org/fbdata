using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	/// Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam> : IDataModelQueryCommand<TDataModel, SqlConnection, TParam>
		where TImpl : IDataModel, TDataModel, new()
	{
		readonly string _selectAll;
		readonly DynamicSql _selectPage;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
		{
			_selectAll = all;
			_selectPage = page;
			_offsets = offsets;
		}

		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? _selectPage.Text : _selectAll;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(_selectPage.BindLimitParameter, SqlDbType.Int) {Value = behavior.PageSize};
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(_selectPage.BindStartRowParameter, SqlDbType.Int) {Value = (page*behavior.PageSize)};
						cmd.Parameters.Add(startRowParam);
					}
				}

				BindCommand((SqlCommand)cmd, _offsets, param);
				using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, _offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(_offsets.Length);
						}
						res.Add(model);
					}
				}
			}
			if (limited)
			{
				return new DataModelQueryResult<TDataModel>(new QueryBehavior(behavior.Behaviors, behavior.PageSize, page, pageCount),
					res);
			}
			return new DataModelQueryResult<TDataModel>(new QueryBehavior(behavior.Behaviors), res);
		}

		/// <summary>
		/// Executes the command with the specified criteria. 
		/// </summary>
		/// <param name="cx">A db context.</param>
		/// <param name="cn">A db connection.</param>
		/// <param name="param">the criteria for the command.</param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(_selectAll, CommandType.Text))
			{
				BindCommand((SqlCommand)cmd, _offsets, param);
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
		/// Implemented by specialized classes to bind the criteria to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="offsets"></param>
		/// <param name="param"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param);
	}


}
