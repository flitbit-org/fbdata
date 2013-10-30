using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	/// Basic query for selecting and paging over models without criteria.
	/// </summary>
	/// <typeparam name="TModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	public class DataModelQueryManyCommand<TModel, TImpl> : IDataModelQueryManyCommand<TModel, SqlConnection>
		where TImpl : TModel, IDataModel, new()
	{
		readonly string _selectAll;
		readonly DynamicSql _selectPage;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public DataModelQueryManyCommand(string all, DynamicSql page, int[] offsets)
		{
			_selectAll = all;
			_selectPage = page;
			_offsets = offsets;
		}

		public IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TModel>();
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
				return new DataModelQueryResult<TModel>(new QueryBehavior(behavior.Behaviors, behavior.PageSize, page, pageCount),
					res);
			}
			return new DataModelQueryResult<TModel>(new QueryBehavior(behavior.Behaviors), res);
		}
	}
}
