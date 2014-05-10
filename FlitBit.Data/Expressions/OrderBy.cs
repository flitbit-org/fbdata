#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
  public class OrderBy
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly List<MappedSortColumn> _columns = new List<MappedSortColumn>();

    public IList<MappedSortColumn> Columns { get { return _columns; } }

    public OrderBy Add(ColumnMapping column, SortOrderKind kind)
    {
      _columns.Add(new MappedSortColumn(column, kind, _columns.Count));
      return this;
    }

    public virtual void WriteOrderBy(IMapping mapping, SqlWriter writer, string refName, bool inverse)
    {
      writer.Append("ORDER BY ");
      var i = 0;
      foreach (var column in _columns)
      {
        if (i > 0)
        {
          writer.Append(", ");
        }
        else
        {
          i += 1;
        }
        column.WriteConditions(mapping, writer, refName, inverse);
      }
    }

    public override string ToString()
    {
      var writer = new StringBuilder();
      writer.Append("ORDER BY ");
      var i = 0;
      foreach (var column in _columns)
      {
        if (i > 0)
        {
          writer.Append(", ");
        }
        else
        {
          i += 1;
        }
        writer.Append(column);
      }
      return writer.ToString();
    }
  }
}