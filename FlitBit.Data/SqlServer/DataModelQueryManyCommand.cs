#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
  /// <summary>
  ///   Basic query for selecting and paging over models without criteria.
  /// </summary>
  /// <typeparam name="TModel">the data model type TModel</typeparam>
  /// <typeparam name="TImpl">the implementation type TImpl</typeparam>
  public class SqlDataModelQueryManyCommand<TModel, TImpl> : IDataModelQueryManyCommand<TModel, SqlConnection>
    where TImpl : TModel, IDataModel, new()
  {
    readonly DynamicSql _selectAll;
    readonly DynamicSql _selectPage;
    readonly int[] _offsets;
    readonly int _countField;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    public SqlDataModelQueryManyCommand(DynamicSql all, DynamicSql page, int[] offsets)
    {
      _selectAll = all;
      _selectPage = page;
      _offsets = offsets;
      _countField = _offsets.Max() + 1;
    }

    public IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior)
    {
      var paging = behavior.IsPaging;
      var limited = behavior.IsLimited;
      // behavior.Page is 1 based, our math is zero-based.
      var page = behavior.Page - 1;
      var res = new List<TModel>();
      var totalRows = 0L;
      cn.EnsureConnectionIsOpen();
      var query = (limited) ? _selectPage : _selectAll;

      using (var cmd = cn.CreateCommand(query.Text, query.CommandType))
      {
        if (limited)
        {
          var limitParam = new SqlParameter(_selectPage.BindLimitParameter, SqlDbType.Int)
          {
            Value = behavior.PageSize
          };
          cmd.Parameters.Add(limitParam);
          if (paging)
          {
            var startRowParam = new SqlParameter(_selectPage.BindStartRowParameter, SqlDbType.Int)
            {
              Value = (page * behavior.PageSize)
            };
            cmd.Parameters.Add(startRowParam);
          }
        }

        using (var reader = cmd.ExecuteReader(query.CommandBehavior))
        {
          cx.IncrementQueryCounter();
          while (reader.Read())
          {
            var model = new TImpl();
            model.LoadFromDataReader(reader, _offsets);
            if (limited && totalRows == 0)
            {
              totalRows = reader.GetInt32(_countField);
            }
            res.Add(model);
          }
          cx.IncrementObjectsFetched(res.Count);
        }
      }
      if (limited)
      {
        // behavior.Page is 1 based, our math is zero-based.
        return
          new DataModelQueryResult<TModel>(
            new QueryBehavior(behavior.Behaviors, behavior.PageSize, page + 1, totalRows),
            res);
      }
      return new DataModelQueryResult<TModel>(new QueryBehavior(behavior.Behaviors), res);
    }
  }
}