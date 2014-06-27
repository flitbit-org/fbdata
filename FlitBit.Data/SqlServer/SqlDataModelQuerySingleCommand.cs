#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
  /// <summary>
  ///   Abstract base; command intended to produce a single instance result.
  /// </summary>
  /// <typeparam name="TDataModel"></typeparam>
  /// <typeparam name="TImpl"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  public abstract class SqlDataModelQuerySingleCommand<TDataModel, TImpl, TParam> :
    IDataModelQuerySingleCommand<TDataModel, SqlConnection, TParam>
    where TImpl : TDataModel, IDataModel, new()
  {
    readonly DynamicSql _sql;
    readonly string _commandText;
    readonly int[] _offsets;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="sql">Initial command text.</param>
    /// <param name="offsets">column offsets within the results returned by the command</param>
    protected SqlDataModelQuerySingleCommand(DynamicSql sql, int[] offsets)
    {
      _sql = sql;
      _commandText = sql.Text;
      _offsets = offsets;
    }

    /// <summary>
    ///   Executes the command with the specified criteria.
    /// </summary>
    /// <param name="cx">A db context.</param>
    /// <param name="cn">A db connection.</param>
    /// <param name="param">the criteria for the command.</param>
    /// <returns>A single data model result.</returns>
    /// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
    public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParam param)
    {
      var res = default(TImpl);
      string cacheKey = null;
      using (var cmd = cn.CreateCommand(_commandText, CommandType.Text))
      {
        BindCommand((SqlCommand)cmd, param, _offsets);
        using (var reader = cmd.ExecuteReader())
        {
          cx.IncrementQueryCounter();
          if (reader.Read())
          {
            res = new TImpl();
            res.LoadFromDataReader(reader, _offsets);
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
    /// <param name="param"></param>
    /// <param name="offsets"></param>
    protected abstract void BindCommand(SqlCommand cmd, TParam param, int[] offsets);
  }
}