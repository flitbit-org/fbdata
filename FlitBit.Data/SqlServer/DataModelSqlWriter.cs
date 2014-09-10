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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly int _bufferLength;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly string _dbObjectReference;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly bool _hasTimestamp;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly bool _hasTimestampOnUpdate;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly ColumnMapping _idCol;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly string _indent;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly IMapping<TDataModel> _mapping =
            DataModel<TDataModel>.Mapping;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly string _selfRef;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Tuple<int, int[]> _columnOffsets;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly bool _hasSyntheticId;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        OrderBy _primaryKeyOrder;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Tuple<int, string[]> _quotedColumnNames;

        public DataModelSqlWriter()
            : this(DefaultSelfName, DefaultIndent)
        {}

        public DataModelSqlWriter(string selfName, string indent)
        {
            Contract.Requires<ArgumentNullException>(selfName != null);
            Contract.Requires<ArgumentException>(selfName.Length > 0);
            Contract.Requires<ArgumentNullException>(indent != null);
            SelfName = selfName;
            _selfRef = Mapping.QuoteObjectName(selfName);
            _dbObjectReference = Mapping.DbObjectReference;
            _indent = indent;
            _bufferLength = _mapping.Columns.Count() * 40;
            _idCol = _mapping.Identity.Columns[0].Column;
            _hasTimestamp = _mapping.Columns.Any(c => c.IsTimestampOnInsert || c.IsTimestampOnUpdate);
            _hasTimestampOnUpdate = _mapping.Columns.Any(c => c.IsTimestampOnUpdate);
            _hasSyntheticId = _mapping.Columns.Any(c => c.IsSynthetic);
        }

        public IMapping<TDataModel> Mapping { get { return _mapping; } }

        public DynamicSql SelectInPrimaryKeyOrder
        {
            get
            {
                var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
                    .Append(Select.Text)
                    .NewLine();
                PrimaryKeyOrder.WriteOrderBy(Mapping, writer, false);
                return new DynamicSql(writer.ToString(), CommandType.Text,
                    CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
            }
        }

        public DynamicSql SelectInPrimaryKeyOrderWithPaging { get { return WriteSelectWithPaging(null); } }

        public OrderBy PrimaryKeyOrder
        {
            get
            {
                return Util.NonBlockingLazyInitializeVolatile(ref _primaryKeyOrder, () =>
                {
                    var res = new OrderBy();
                    foreach (var id in Mapping.Identity.Columns)
                    {
                        var idex = new SqlValueExpression(SqlExpressionKind.MemberAccess,
                            String.Concat(this.SelfRef, ".", Mapping.QuoteObjectName(id.Column.TargetName)),
                            id.Column.RuntimeType
                            );
                        res.Add(idex, SortOrderKind.Asc);
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
                var mapping = Mapping;
                var helper = mapping.GetDbProviderHelper();
                var idStr = helper.FormatParameterName("@identity");
                var res = new DynamicSql(null, CommandType.Text,
                    CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
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
                if (!_hasSyntheticId)
                {
                    writer.Append(" = ")
                          .Append(helper.FormatParameterName(_idCol.DbTypeDetails.BindingName));
                }
                writer.NewLine();
                writer.NewLine("INSERT INTO ").Append(_dbObjectReference).Append(" (").Indent();
                writer.NewLine("{0}");
                writer.Outdent().NewLine(")").NewLine("VALUES (").Indent();
                writer.NewLine("{1}");
                writer.Outdent().NewLine(")");
                if (_hasSyntheticId)
                {
                    writer.NewLine("SET ").Append(idStr).Append(" = SCOPE_IDENTITY()");
                }
                writer.NewLine(Select.Text)
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

                var helper = Mapping.GetDbProviderHelper();
                var idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
                var res = new DynamicSql(null, CommandType.Text,
                    CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
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
                var helper = Mapping.GetDbProviderHelper();
                var idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
                var res = new DynamicSql(null, CommandType.Text,
                    CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
                {
                    BindIdentityParameter = idStr
                };
                var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
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
                var helper = Mapping.GetDbProviderHelper();
                var idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
                var res = new DynamicSql(null, CommandType.Text,
                    CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
                {
                    BindIdentityParameter = idStr
                };
                var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
                    .Append("DELETE FROM ").Append(Mapping.DbObjectReference)
                    .NewLine("WHERE ").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr);
                res.Text = writer.ToString();
                return res;
            }
        }

        public string SelfRef { get { return _selfRef; } }

        public int[] ColumnOffsets
        {
            get
            {
                var m = Mapping;
                if (_columnOffsets == null
                    || _columnOffsets.Item1 < m.Revision)
                {
                    var helper = m.GetDbProviderHelper();
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
                var m = Mapping;
                if (_quotedColumnNames == null
                    || _quotedColumnNames.Item1 < m.Revision)
                {
                    var helper = m.GetDbProviderHelper();
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
                var mapping = Mapping;
                var helper = mapping.GetDbProviderHelper();
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
            var writer = new SqlWriter().Append(Select.Text);
            sql.Write(writer);
            return new DynamicSql(writer.Text, CommandType.Text,
                CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
        }

        public DynamicSql WriteSelectWithPaging(DataModelSqlExpression<TDataModel> sql)
        {
            var mapping = Mapping;
            // Default to PK order...
            var order = sql.OrderByStatement(this.PrimaryKeyOrder);

            var res = new DynamicSql(null, CommandType.Text,
                CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
            res.BindLimitParameter = "@pageSize";
            res.BindStartRowParameter = "@startRow";

            var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
            var colList = QuotedColumnNames.Select(c => String.Concat(_selfRef, ".", c)).ToArray();

            writer.Append("SELECT TOP (@pageSize)");
            AppendColumns(writer, colList);
            writer.Append(",").NewLine(_selfRef).Append(".[$$RowCount]").Outdent()
                  .NewLine("FROM (").Indent().NewLine("SELECT ");
            AppendColumns(writer, colList);
            writer.Append(",").NewLine("ROW_NUMBER() OVER(");
            var orderByStatement = new SqlWriter();
            order.WriteOrderBy(mapping, orderByStatement, false);
            writer.Append(orderByStatement.ToString()).Append(") AS [$$RowNumber],")
                  .NewLine("COUNT(*) OVER(PARTITION BY 1) AS [$$RowCount]")
                  .Outdent()
                  .NewLine("FROM ").Append(_dbObjectReference).Append(" AS ").Append(_selfRef);
            if (sql != null)
            {
                sql.Write(writer);
            }
            writer.Outdent().NewLine(") AS ").Append(_selfRef)
                  .NewLine("WHERE ").Append(_selfRef).Append(".[$$RowNumber] >= @startRow");

            res.Text = writer.Text;
            return res;
        }

        void AppendColumns(SqlWriter writer, IEnumerable<string> colums)
        {
            var i = 0;
            foreach (var c in colums)
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

        void AppendColumns(SqlWriter writer, IEnumerable<ColumnMapping> columns,
            Func<ColumnMapping, string> eaColumn)
        {
            var i = 0;
            foreach (var c in columns.Select(eaColumn))
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

        public string SelfName { get; private set; }
    }
}