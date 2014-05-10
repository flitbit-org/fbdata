#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using FlitBit.Data.DataModel;

namespace FlitBit.Data.SqlServer
{
  /// <summary>
  ///   Basic data model query command for queries with one parameter.
  /// </summary>
  public abstract class SqlDataModelCommand
  {
    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    protected SqlDataModelCommand(DynamicSql all, DynamicSql page, int[] offsets)
    {
      AllQuery = all;
      PagingQuery = page;
      Offsets = offsets;
    }

    /// <summary>
    ///   The query's text.
    /// </summary>
    protected DynamicSql AllQuery { get; private set; }

    /// <summary>
    ///   The paging query.
    /// </summary>
    protected DynamicSql PagingQuery { get; private set; }

    /// <summary>
    ///   An array of offsets.
    /// </summary>
    protected int[] Offsets { get; private set; }
  }
}