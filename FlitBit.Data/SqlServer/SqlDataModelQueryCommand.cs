﻿using System.Collections.Generic;
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
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam>
		where TImpl : IDataModel, TDataModel, new()
	{
		
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets) : base(all, page, offsets)
		{
		}

		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param);
				using (var reader = cmd.ExecuteReader())
				{
					cx.IncrementQueryCounter();
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param);
				using (var reader = cmd.ExecuteReader())
				{
					cx.IncrementQueryCounter();
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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

	/// <summary>
	/// Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1);
	}

	/// <summary>
	/// Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2);
	}

	/// <summary>
	/// Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2, TParam3 param3);
	}

	/// <summary>
	/// Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
	}

	/// <summary>
	/// Basic data model query command for queries with one parameter.
	/// </summary>
	/// <typeparam name="TDataModel">the data model type TModel</typeparam>
	/// <typeparam name="TImpl">the implementation type TImpl</typeparam>
	/// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);
	}


	/// <summary>
	/// Basic data model query command for queries with one parameter.
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
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6);
	}
	
	/// <summary>
	/// Basic data model query command for queries with one parameter.
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
	/// <typeparam name="TParam7"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7);
	}
	

	/// <summary>
	/// Basic data model query command for queries with one parameter.
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
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7, param8);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7, param8);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8);
	}
	
	/// <summary>
	/// Basic data model query command for queries with one parameter.
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
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	/// <typeparam name="TParam9"></typeparam>
	public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> : SqlDataModelCommand,
		IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
		where TImpl : IDataModel, TDataModel, new()
	{

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected SqlDataModelQueryCommand(string all, DynamicSql page, int[] offsets)
			: base(all, page, offsets)
		{
		}

		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="param">criteria used to bind the command.</param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <param name="param9"></param>
		/// <returns>a data model query result</returns>
		public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var page = behavior.Page - 1;
			var res = new List<TDataModel>();
			var pageCount = 0;
			cn.EnsureConnectionIsOpen();
			var query = (limited) ? PagingQuery.Text : AllQuery;

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (limited)
				{
					var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(limitParam);
					if (paging)
					{
						var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int) { Value = (page * behavior.PageSize) };
						cmd.Parameters.Add(startRowParam);
					}
				}
				var offsets = Offsets;
				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7, param8, param9);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var model = new TImpl();
						model.LoadFromDataReader(reader, offsets);
						if (limited && pageCount == 0)
						{
							pageCount = reader.GetInt32(offsets.Length);
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
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <param name="param9"></param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
		{
			cn.EnsureConnectionIsOpen();
			var res = default(TImpl);
			using (var cmd = cn.CreateCommand(AllQuery, CommandType.Text))
			{
				var offsets = Offsets;

				BindCommand((SqlCommand)cmd, offsets, param, param1, param2, param3, param4, param5, param6, param7, param8, param9);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, offsets);
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
		/// <param name="param1"></param>
 		/// <param name="param2"></param>
 		/// <param name="param3"></param>
 		/// <param name="param4"></param>
 		/// <param name="param5"></param>
 		/// <param name="param6"></param>
 		/// <param name="param7"></param>
 		/// <param name="param8"></param>
 		/// <param name="param9"></param>
		protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9);
	}
}