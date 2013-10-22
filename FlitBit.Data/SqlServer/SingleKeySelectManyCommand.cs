﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	/// Basic select many for a type that has a single primary key column.
	/// </summary>
	/// <typeparam name="TModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	public class SingleKeySelectManyCommand<TModel, TImpl> : IDataModelQueryManyCommand<TModel, SqlConnection>
		where TImpl : TModel, IDataModel, new()
	{
		readonly string _selectAll;
		readonly string _selectPage;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public SingleKeySelectManyCommand(string selectAll, string selectPage, int[] offsets)
		{
			if (selectAll == null)
			{
				throw new ArgumentNullException("selectAll");
			}
			if (selectPage == null)
			{
				throw new ArgumentNullException("selectPage");
			}
			if (offsets == null)
			{
				throw new ArgumentNullException("offsets");
			}
			_selectAll = selectAll;
			_selectPage = selectPage;
			_offsets = offsets;
		}

		public IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page;
			var res = new List<TModel>();
			var pageCount = 0;
			QueryBehavior outbehavior = null;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? _selectPage : _selectAll;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter("@limit", SqlDbType.Int) {Value = behavior.PageSize};
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter("@startRow", SqlDbType.Int) {Value = (page - 1)*behavior.PageSize};
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
