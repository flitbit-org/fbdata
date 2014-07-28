#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
  /// <summary>
  ///   Basic data model query command for queries with one parameter.
  /// </summary>
  /// <typeparam name="TDataModel">the data model type TModel</typeparam>
  /// <typeparam name="TImpl">the implementation type TImpl</typeparam>
  /// <typeparam name="TParam">typeof of param zero bound to the command</typeparam>
  /// <typeparam name="TParam1"></typeparam>
  public abstract class SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1> : SqlDataModelCommand,
    IDataModelQueryCommand<TDataModel, SqlConnection, TParam, TParam1>
    where TImpl : IDataModel, TDataModel, new()
  {
    readonly Func<TParam, TParam1, string> _formatSingleCacheKey;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    protected SqlDataModelQueryCommand(DynamicSql all, DynamicSql page, int[] offsets)
      : base(all, page, offsets)
    {
      _formatSingleCacheKey = all.FormatSingleCacheKey as Func<TParam, TParam1, string>;
    }

    /// <summary>
    ///   Executes the query using the specified criteria, on the specified connection, according to the specified behavior
    ///   (possibly paging).
    /// </summary>
    /// <param name="cx">the db context</param>
    /// <param name="cn">a db connection used to execute the command</param>
    /// <param name="behavior">behaviors, possibly paging</param>
    /// <param name="param">criteria used to bind the command.</param>
    /// <param name="param1"></param>
    /// <returns>a data model query result</returns>
    public IDataModelQueryResult<TDataModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior,
      TParam param, TParam1 param1)
    {
      var paging = behavior.IsPaging;
      var limited = behavior.IsLimited;
      var page = behavior.Page - 1;
      var res = new List<TDataModel>();
      var totalRows = 0L;
      cn.EnsureConnectionIsOpen();
      var query = (limited) ? PagingQuery : AllQuery;

      using (var cmd = cn.CreateCommand(query.Text, query.CommandType))
      {
        if (limited)
        {
          var limitParam = new SqlParameter(PagingQuery.BindLimitParameter, SqlDbType.Int)
          {
            Value = behavior.PageSize
          };
          cmd.Parameters.Add(limitParam);
          if (paging)
          {
            var startRowParam = new SqlParameter(PagingQuery.BindStartRowParameter, SqlDbType.Int)
            {
              Value = (page * behavior.PageSize)
            };
            cmd.Parameters.Add(startRowParam);
          }
        }
        var offsets = Offsets;
        BindCommand((SqlCommand)cmd, offsets, param, param1);
        using (var reader = cmd.ExecuteReader(query.CommandBehavior))
        {
          while (reader.Read())
          {
            var model = new TImpl();
            model.LoadFromDataReader(reader, offsets);
            if (limited && totalRows == 0)
            {
              totalRows = reader.GetInt32(offsets.Length);
            }
            res.Add(model);
          }
        }
      }
      if (limited)
      {
        return
          new DataModelQueryResult<TDataModel>(
            new QueryBehavior(behavior.Behaviors, behavior.PageSize, behavior.Page, totalRows),
            res);
      }
      return new DataModelQueryResult<TDataModel>(new QueryBehavior(behavior.Behaviors), res);
    }

    /// <summary>
    ///   Executes the command with the specified criteria.
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
      var query = AllQuery;
      var res = default(TImpl);
      using (var cmd = cn.CreateCommand(query.Text, query.CommandType))
      {
        var offsets = Offsets;

        BindCommand((SqlCommand)cmd, offsets, param, param1);
        using (var reader = cmd.ExecuteReader(query.CommandBehavior))
        {
          if (reader.Read())
          {
            res = new TImpl();
            res.LoadFromDataReader(reader, offsets);
            cx.IncrementObjectsFetched();
          }
          if (reader.Read())
          {
            throw new DuplicateObjectException();
          }
        }
      }
      return res;
    }

    /// <summary>
    ///   Implemented by specialized classes to bind the criteria to the command.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="offsets"></param>
    /// <param name="param"></param>
    /// <param name="param1"></param>
    protected abstract void BindCommand(SqlCommand cmd, int[] offsets, TParam param, TParam1 param1);
  }

}