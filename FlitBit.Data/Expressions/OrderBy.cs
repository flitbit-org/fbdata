#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
  public class OrderByColumn
  {
    public OrderByColumn(SqlValueExpression expr, SortOrderKind kind)
    {
      this.Expression = expr;
      this.Kind = kind;
    }

    public SortOrderKind Kind { get; set; }

    public SqlValueExpression Expression { get; set; }

    public void WriteConditions(IMapping mapping, SqlWriter writer, bool inverse)
    {
      writer.Append(mapping.QuoteObjectName(Expression.Text));
      if (inverse)
      {
        writer.Append(Kind == SortOrderKind.Asc ? " DESC" : " ASC");
      }
      else
      {
        writer.Append(Kind == SortOrderKind.Asc ? " ASC" : " DESC");
      }
    }

    public override string ToString()
    {
      var writer = new StringBuilder(40);
      writer.Append(Expression.Text);
      writer.Append(Kind == SortOrderKind.Asc ? " ASC" : " DESC");
      return writer.ToString();
    }
  }

  public class OrderBy
  {
    
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly List<OrderByColumn> _columns = new List<OrderByColumn>();

    public IList<OrderByColumn> Columns { get { return _columns; } }

    public OrderBy Add(SqlValueExpression column, SortOrderKind kind)
    {
      _columns.Add(new OrderByColumn(column, kind));
      return this;
    }

    public virtual void WriteOrderBy(IMapping mapping, SqlWriter writer, bool inverse)
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
        column.WriteConditions(mapping, writer, inverse);
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