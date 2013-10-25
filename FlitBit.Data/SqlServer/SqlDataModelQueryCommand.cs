using System;
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
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TCriteria">typeof of criteria bound to the command</typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TCriteria> : IDataModelQueryCommand<TDataModel, SqlConnection, TCriteria>
		where TImpl : IDataModel, TDataModel, new()
	{
		readonly string _selectAll;
		readonly string _selectPage;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string selectAll, string selectPage, int[] offsets)
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

		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TCriteria criteria)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page;
			var res = new List<TDataModel>();
			var pageCount = 0;
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

				BindCommand((SqlCommand)cmd, criteria, _offsets);
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
		/// <param name="criteria">the criteria for the command.</param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TCriteria criteria)
		{
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(_selectAll, CommandType.Text))
			{
				BindCommand((SqlCommand)cmd, criteria, _offsets);
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
		/// <param name="criteria"></param>
		/// <param name="offsets"></param>
		protected abstract void BindCommand(SqlCommand cmd, TCriteria criteria, int[] offsets);
	}
}
