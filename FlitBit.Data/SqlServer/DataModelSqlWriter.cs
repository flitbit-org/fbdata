#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
  public class DataModelSqlWriter<TDataModel> : IDataModelWriter<TDataModel>
  {
    public static readonly string DefaultSelfName = "self";
    public static readonly string DefaultIndent = "  ";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _bufferLength;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _dbObjectReference;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly bool _hasTimestamp;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly bool _hasTimestampOnUpdate;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ColumnMapping _idCol;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _indent;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly IMapping<TDataModel> _mapping =
      DataModel<TDataModel>.Mapping;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _selfRef;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Tuple<int, int[]> _columnOffsets;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _hasSyntheticId;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private OrderBy _primaryKeyOrder;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Tuple<int, string[]> _quotedColumnNames;

    public DataModelSqlWriter()
      : this(DefaultSelfName, DefaultIndent)
    {
    }

    public DataModelSqlWriter(string selfName, string indent)
    {
      Contract.Requires<ArgumentNullException>(selfName != null);
      Contract.Requires<ArgumentException>(selfName.Length > 0);
      Contract.Requires<ArgumentNullException>(indent != null);
      SelfName = selfName;
      _selfRef = Mapping.QuoteObjectName(selfName);
      _dbObjectReference = Mapping.DbObjectReference;
      _indent = indent;
      _bufferLength = _mapping.Columns.Count()*40;
      _idCol = _mapping.Identity.Columns[0].Column;
      _hasTimestamp = _mapping.Columns.Any(c => c.IsTimestampOnInsert || c.IsTimestampOnUpdate);
      _hasTimestampOnUpdate = _mapping.Columns.Any(c => c.IsTimestampOnUpdate);
      _hasSyntheticId = _mapping.Columns.Any(c => c.IsSynthetic);
    }

    public IMapping<TDataModel> Mapping
    {
      get { return _mapping; }
    }

    public DynamicSql SelectInPrimaryKeyOrder
    {
      get
      {
        SqlWriter writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
          .Append(Select.Text)
          .NewLine();
        PrimaryKeyOrder.WriteOrderBy(Mapping, writer, _selfRef, false);
        return new DynamicSql(writer.ToString(), CommandType.Text,
          CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
      }
    }

    public DynamicSql SelectInPrimaryKeyOrderWithPaging
    {
      get { return WriteSelectWithPaging(null, null); }
    }

    public OrderBy PrimaryKeyOrder
    {
      get
      {
        return Util.NonBlockingLazyInitializeVolatile(ref _primaryKeyOrder, () =>
        {
          var res = new OrderBy();
          foreach (MappedSortColumn id in Mapping.Identity.Columns)
          {
            res.Add(id.Column, id.Kind);
          }
          return res;
        });
      }
    }

    public DynamicSql DynamicInsertStatement
    {
      get
      {
        Contract.Ensures(Contract.Result<DynamicSql>() != null);
        IMapping<TDataModel> mapping = Mapping;
        DbProviderHelper helper = mapping.GetDbProviderHelper();
        string idStr = helper.FormatParameterName("@identity");
        var res = new DynamicSql(null, CommandType.Text, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
        {
          SyntheticIdentityVar = idStr
        };
        var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
        if (_hasTimestamp)
        {
          res.CalculatedTimestampVar = "@generated_timestamp";
          writer.NewLine("DECLARE @generated_timestamp DATETIME2 = GETUTCDATE()");
        }
        writer.NewLine();
        _idCol.Emitter.DeclareScriptVariable(writer, _idCol, helper, idStr);
        writer.NewLine();
        writer.NewLine("INSERT INTO ").Append(_dbObjectReference).Append(" (").Indent();
        writer.NewLine("{0}");
        writer.Outdent().NewLine(")").NewLine("VALUES (").Indent();
        writer.NewLine("{1}");
        writer.Outdent().NewLine(")")
          .NewLine("SET ").Append(idStr).Append(" = SCOPE_IDENTITY()")
          .NewLine(Select.Text)
          .NewLine("WHERE ")
          .Append(_selfRef)
          .Append(".")
          .Append(helper.QuoteObjectName(_idCol.TargetName))
          .Append(" = ")
          .Append(idStr);
        res.Text = writer.ToString();
        return res;
      }
    }

    public DynamicSql DynamicUpdateStatement
    {
      get
      {
        Contract.Ensures(Contract.Result<DynamicSql>() != null);

        DbProviderHelper helper = Mapping.GetDbProviderHelper();
        string idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
        var res = new DynamicSql(null, CommandType.Text, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
        {
          BindIdentityParameter = idStr
        };
        var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
        if (_hasTimestampOnUpdate)
        {
          res.CalculatedTimestampVar = "@generated_timestamp";
          writer.NewLine("DECLARE @generated_timestamp DATETIME2 = GETUTCDATE()");
        }
        writer.NewLine();
        writer.NewLine("UPDATE ").Append(_dbObjectReference).Indent()
          .NewLine("SET {0}");
        writer
          .NewLine("WHERE ").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr)
          .NewLine().Outdent()
          .NewLine(Select.Text)
          .NewLine("WHERE ")
          .Append(_selfRef)
          .Append(".")
          .Append(helper.QuoteObjectName(_idCol.TargetName))
          .Append(" = ")
          .Append(idStr);
        res.Text = writer.ToString();
        return res;
      }
    }

    public DynamicSql SelectByPrimaryKey
    {
      get
      {
        DbProviderHelper helper = Mapping.GetDbProviderHelper();
        string idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
        var res = new DynamicSql(null, CommandType.Text, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
        {
          BindIdentityParameter = idStr
        };
        SqlWriter writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
          .Append(Select.Text)
          .NewLine("WHERE ").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr);
        res.Text = writer.ToString();
        return res;
      }
    }

    public DynamicSql DeleteByPrimaryKey
    {
      get
      {
        DbProviderHelper helper = Mapping.GetDbProviderHelper();
        string idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
        var res = new DynamicSql(null, CommandType.Text, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
        {
          BindIdentityParameter = idStr
        };
        SqlWriter writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
          .Append("DELETE FROM ").Append(Mapping.DbObjectReference)
          .NewLine("WHERE ").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr);
        res.Text = writer.ToString();
        return res;
      }
    }

    public string SelfRef
    {
      get { return _selfRef; }
    }

    public int[] ColumnOffsets
    {
      get
      {
        IMapping<TDataModel> m = Mapping;
        if (_columnOffsets == null || _columnOffsets.Item1 < m.Revision)
        {
          DbProviderHelper helper = m.GetDbProviderHelper();
          _columnOffsets = Tuple.Create(m.Revision,
            Enumerable.Range(0, m.Columns.Count()).ToArray()
            );
        }
        return _columnOffsets.Item2;
      }
    }

    public string[] QuotedColumnNames
    {
      get
      {
        IMapping<TDataModel> m = Mapping;
        if (_quotedColumnNames == null || _quotedColumnNames.Item1 < m.Revision)
        {
          DbProviderHelper helper = m.GetDbProviderHelper();
          _quotedColumnNames = Tuple.Create(m.Revision,
            m.Columns
              .OrderBy(c => c.Ordinal)
              .Select(c => helper.QuoteObjectName(c.TargetName)).ToArray()
            );
        }
        return _quotedColumnNames.Item2;
      }
    }

    public DynamicSql Select
    {
      get
      {
        IMapping<TDataModel> mapping = Mapping;
        DbProviderHelper helper = mapping.GetDbProviderHelper();
        var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
        writer.Append("SELECT ");
        AppendColumns(writer, mapping.Columns.OrderBy(c => c.Ordinal),
          c => String.Concat(_selfRef, ".", helper.QuoteObjectName(c.TargetName)));
        writer.Outdent().NewLine()
          .Append("FROM ").Append(_dbObjectReference)
          .Append(" AS ").Append(_selfRef);

        return new DynamicSql(writer.ToString(), CommandType.Text,
          CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
      }
    }

    public DynamicSql WriteSelect(DataModelSqlExpression<TDataModel> sql)
    {
      SqlWriter writer = new SqlWriter().Append(Select.Text);
      sql.Write(writer);
      return new DynamicSql(writer.Text, CommandType.Text,
        CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
    }

    public DynamicSql WriteSelectWithPaging(DataModelSqlExpression<TDataModel> sql, OrderBy orderBy)
    {
      IMapping<TDataModel> mapping = Mapping;
      // Default to PK order...
      OrderBy order = orderBy ?? PrimaryKeyOrder;

      var res = new DynamicSql(null, CommandType.Text, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
      res.BindLimitParameter = "@pageSize";
      res.BindStartRowParameter = "@startRow";

      var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
      string[] colList = QuotedColumnNames.Select(c => String.Concat(_selfRef, ".", c)).ToArray();

      writer.Append("DECLARE @endRow INT = @startRow + (@pageSize - 1);")
        .NewLine("WITH dataset AS(").Indent()
        .NewLine("SELECT ");
      AppendColumns(writer, colList);
      writer.Append(",").NewLine("ROW_NUMBER() OVER(");
      var orderByStatement = new SqlWriter();
      var inverseOrderByStatement = new SqlWriter();
      order.WriteOrderBy(mapping, orderByStatement, _selfRef, false);
      order.WriteOrderBy(mapping, inverseOrderByStatement, _selfRef, true);
      writer.Append(orderByStatement.ToString()).Append(") AS seq,")
        .NewLine("ROW_NUMBER() OVER(")
        .Append(inverseOrderByStatement.ToString())
        .Append(") AS rev_seq")
        .Outdent()
        .NewLine()
        .Append("FROM ").Append(_dbObjectReference).Append(" AS ").Append(_selfRef);
      if (sql != null)
      {
        sql.Write(writer);
      }
      writer.Outdent().NewLine(")")
        .NewLine(@"SELECT TOP (@pageSize) ");
      AppendColumns(writer, colList);
      writer.Append(",").NewLine("rev_seq + seq -1 as [RowCount]")
        .Outdent().NewLine("FROM dataset AS [self]")
        .NewLine("WHERE seq >= @startRow")
        .NewLine("ORDER BY seq");

      res.Text = writer.Text;
      return res;
    }

    private void AppendColumns(SqlWriter writer, IEnumerable<string> colums)
    {
      int i = 0;
      foreach (string c in colums)
      {
        if (i++ > 0)
        {
          writer.Append(',').NewLine(c);
        }
        else
        {
          writer.Append(c);
          writer.Indent();
        }
      }
    }

    private void AppendColumns(SqlWriter writer, IEnumerable<ColumnMapping> columns,
      Func<ColumnMapping, string> eaColumn)
    {
      int i = 0;
      foreach (string c in columns.Select(eaColumn))
      {
        if (i++ > 0)
        {
          writer.Append(',').NewLine(c);
        }
        else
        {
          writer.Append(c);
          writer.Indent();
        }
      }
    }

    public string SelfName
    {
      get; private set;
    }
  }
}