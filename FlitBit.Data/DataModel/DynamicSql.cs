﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Dynamic;

namespace FlitBit.Data.DataModel
{
  /// <summary>
  ///   A dynamic SQL statement, generated by the framework for an operation on a data model.
  /// </summary>
  public class DynamicSql
  {
    /// <summary>
    ///   Creates a new empty SQL statement.
    /// </summary>
    public DynamicSql()
      : this(null, CommandType.Text) { }

    /// <summary>
    ///   Creates the specified SQL statement.
    /// </summary>
    /// <param name="sql"></param>
    public DynamicSql(string sql)
      : this(sql, CommandType.Text) { }

    /// <summary>
    ///   Creates the specified SQL statement.
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="cmdType"></param>
    public DynamicSql(string sql, CommandType cmdType)
      : this(sql, cmdType, CommandBehavior.Default)
    {
    }

    /// <summary>
    ///   Creates the specified SQL statement.
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="cmdType"></param>
    /// <param name="behavior"></param>
    public DynamicSql(string sql, CommandType cmdType, CommandBehavior behavior)
    {
      Text = sql;
      CommandType = cmdType;
      CommandBehavior = behavior;
    }

    /// <summary>
    ///   For INSERT statements, indicates the name of the synthetic ID variable within the statement.
    /// </summary>
    public string SyntheticIdentityVar { get; set; }

    /// <summary>
    ///   For INSERT and UPDATE statements, indicates the name of the calculated timestamp variable within the statement.
    /// </summary>
    public string CalculatedTimestampVar { get; set; }

    /// <summary>
    ///   Gets the text of the statement (unbound).
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    ///   Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///   A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override string ToString() { return Text; }

    /// <summary>
    ///   For identity based operations/statements, indicates the name of the identity parameter that must be bound for the
    ///   statement.
    /// </summary>
    public string BindIdentityParameter { get; set; }

    /// <summary>
    ///   For paging and limited results, indicated the name of the limit parameter that must be bound for the statement. Limit
    ///   serves as the page size parameter when paging.
    /// </summary>
    public string BindLimitParameter { get; set; }

    /// <summary>
    ///   For paging, indicated the name of the starting row parameter that must be bound for the statement.
    /// </summary>
    public string BindStartRowParameter { get; set; }

    /// <summary>
    ///   The statement's command type.
    /// </summary>
    public CommandType CommandType { get; private set; }

    /// <summary>
    ///   The statement's command behavior.
    /// </summary>
    public CommandBehavior CommandBehavior { get; private set; }
    
    /// <summary>
    /// A delegate capable of formatting cache keys.
    /// </summary>
    public Delegate FormatSingleCacheKey { get; set; }

    public TimeSpan SingleCacheTimeToLive { get; set; }
  }
}