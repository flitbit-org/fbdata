using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.Meta.DDL;

namespace FlitBit.Data.SqlServer
{
	public class DataModelSqlWriter<TDataModel>
	{
		public static readonly string DefaultSelfName = "self";
		public static readonly string DefaultIndent = "  ";

		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _bufferLength;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly bool _hasTimestamp;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ColumnMapping _idCol;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _indent;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Mapping<TDataModel> _mapping =
			Mapping<TDataModel>.Instance;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _selfRef;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _hasSyntheticId;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _hasTimestampOnUpdate;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private OrderBy _primaryKeyOrder;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Tuple<int, string[]> _quotedColumnNames;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Tuple<int, int[]> _columnOffsets;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]private readonly string _dbObjectReference;

		public DataModelSqlWriter()
			: this(DefaultSelfName, DefaultIndent)
		{
		}

		public DataModelSqlWriter(string selfName, string indent)
		{
			Contract.Requires<ArgumentNullException>(selfName != null);
			Contract.Requires<ArgumentException>(selfName.Length > 0);
			Contract.Requires<ArgumentNullException>(indent != null);
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
				IMapping<TDataModel> m = Mapping;
				if (_quotedColumnNames == null || _quotedColumnNames.Item1 < m.Revision)
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

		public string Select
		{
			get
			{
				IMapping<TDataModel> mapping = Mapping;
				var helper = mapping.GetDbProviderHelper();
				var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
				writer.Append("SELECT ");
				AppendColumns(writer, mapping.Columns.OrderBy(c => c.Ordinal),
					c => String.Concat(_selfRef, ".", helper.QuoteObjectName(c.TargetName)));
				writer.Outdent().NewLine()
					.Append("FROM ").Append(_dbObjectReference)
					.Append(" AS ").Append(_selfRef);
				return writer.ToString();
			}
		}

		public string SelectInPrimaryKeyOrder
		{
			get
			{
				SqlWriter writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
					.Append(Select)
					.NewLine();
				PrimaryKeyOrder.WriteOrderBy(Mapping, writer, _selfRef, false);
				return writer.ToString();
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
				var mapping = Mapping;
				var helper = mapping.GetDbProviderHelper();
				var idStr = helper.FormatParameterName("@identity");
				var res = new DynamicSql() {SyntheticIdentityVar = idStr};
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
					.NewLine(Select)
					.NewLine("WHERE ").Append(_selfRef).Append(".").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr);
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
				var res = new DynamicSql() { BindIdentityParameter = idStr };
				var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
				if (_hasTimestamp)
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
					.NewLine(Select)
					.NewLine("WHERE ").Append(_selfRef).Append(".").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr);
				res.Text = writer.ToString();
				return res;
			}
		}

		public DynamicSql WriteUpdate(Constraints cns)
		{
			Contract.Ensures(Contract.Result<DynamicSql>() != null);

			var helper = Mapping.GetDbProviderHelper();
			var idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
			var res = new DynamicSql() { BindIdentityParameter = idStr };
			var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
			if (_hasTimestamp)
			{
				res.CalculatedTimestampVar = "@generated_timestamp";
				writer.NewLine("DECLARE @generated_timestamp DATETIME2 = GETUTCDATE()");
			}
			writer.NewLine();
			writer.NewLine("UPDATE ").Append(_dbObjectReference).Indent()
				.NewLine("SET {0}");
			writer
				.NewLine("WHERE ").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr);
			res.Text = writer.ToString();
			return res;
		}

		public DynamicSql WriteDeleteWhere(Constraints cns)
		{
			Contract.Ensures(Contract.Result<DynamicSql>() != null);

			var res = new DynamicSql();
			var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
			writer.NewLine("DELETE ");
			PrepareFromAndWhereStatement(_selfRef, cns, writer);
			res.Text = writer.ToString();
			return res;
		}

		public DynamicSql SelectByPrimaryKey
		{
			get
			{
				var helper = Mapping.GetDbProviderHelper();
				var idStr = helper.FormatParameterName(_idCol.DbTypeDetails.BindingName);
				var res = new DynamicSql() { BindIdentityParameter = idStr };
				var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
					.Append(Select)
					.NewLine("WHERE ").Append(helper.QuoteObjectName(_idCol.TargetName)).Append(" = ").Append(idStr);
				res.Text = writer.ToString();
				return res;
			}
		}

		public DynamicSql WriteSelectWithPaging(Constraints cns, OrderBy orderBy)
		{
			IMapping<TDataModel> mapping = Mapping;
			var helper = Mapping.GetDbProviderHelper();
			// Default to PK order...
			OrderBy order = orderBy ?? PrimaryKeyOrder;

			var res = new DynamicSql();
			res.BindLimitParameter = "@limit";
			res.BindStartRowParameter = "@startRow";

			var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
			string[] colList = QuotedColumnNames.Select(c => String.Concat(_selfRef, ".", c)).ToArray();

			writer.Append("DECLARE @endRow INT = @startRow + (@pageSize - 1);")
				.NewLine("WITH dataset AS(").Indent()
				.NewLine("SELECT ");
			AppendColumns(writer, colList);
			writer.NewLine("ROW_NUMBER() OVER(");
			var orderByStatement = new SqlWriter();
			var inverseOrderByStatement = new SqlWriter();
			order.WriteOrderBy(mapping, orderByStatement, _selfRef, false);
			order.WriteOrderBy(mapping, inverseOrderByStatement, _selfRef, true);
			writer.Append(orderByStatement.ToString()).Append(") AS seq,")
				.NewLine("ROW_NUMBER() OVER(")
				.Append(inverseOrderByStatement.ToString())
				.Append(") AS rev_seq,")
				.Outdent();
			PrepareFromAndWhereStatement(_selfRef, cns, writer);
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

		public string WriteSelect(Constraints cns, OrderBy orderBy)
		{
			return WriteSelect(_selfRef, cns, orderBy);
		}

		public string WriteSelect(string refName, Constraints cns, OrderBy orderBy)
		{
			IMapping<TDataModel> mapping = Mapping;
			var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
			writer.Append("SELECT ");
			AppendColumns(writer, QuotedColumnNames.Select(c => String.Concat(refName, ".", c)));
			writer.Outdent();
			PrepareFromAndWhereStatement(refName, cns, writer);
			if (orderBy != null)
			{
				orderBy.WriteOrderBy(mapping, writer, refName, false);
			}
			return writer.ToString();
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

		private void AppendColumns(SqlWriter writer, IEnumerable<ColumnMapping> columns, Func<ColumnMapping, string> eaColumn)
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

		public void PrepareFromAndWhereStatement(string refName, Constraints cns, SqlWriter writer)
		{
			// Perform necessary joins and write join clauses...
			foreach (Join join in cns.Joins.Values.OrderBy(j => j.Ordinal))
			{
				var stack = new Stack<Tuple<Condition, bool>>();
				ProcessConditionsFor(join, cns.Conditions, stack);
				MapJoinFrom(refName, join, cns);
			}
			// Write the primary statement's conditions...
			Condition c = cns.Conditions;
			if (c != null && !c.IsLifted)
			{
				writer.NewLine().Indent().Append("WHERE ");
				c.WriteConditions(Mapping<TDataModel>.Instance, writer);
				writer.Outdent();
			}
		}

		private void ProcessConditionsFor(Join join, Condition condition, Stack<Tuple<Condition, bool>> path)
		{
			if (condition != null)
			{
				if (condition.IsLiftCandidateFor(join))
				{
					join.AddCondition(condition);
				}
				else
				{
					if (condition.Kind == ConditionKind.AndAlso)
					{
						ProcessConditionsFor(join, ((AndCondition) condition).Left, path);
						ProcessConditionsFor(join, ((AndCondition) condition).Right, path);
					}
				}
			}
		}

		private void MapJoinFrom(string refName, Join join, Constraints cns)
		{
			SqlWriter writer = cns.Writer;
			Join jj = join;
			var joins = new Stack<Join>();
			while (jj != null)
			{
				joins.Push(jj);
				jj = jj.Path;
			}
			Mapping<TDataModel> mapping = Mapping<TDataModel>.Instance;
			IMapping fromMapping = mapping;
			string fromRef = refName;
			foreach (Join j in joins)
			{
				IMapping toMapping = j.Mapping;
				ColumnMapping fromCol = fromMapping.Columns.Single(c => c.Member == j.Member);
				string toRef = mapping.QuoteObjectName(Convert.ToString(j.Ordinal));
				ColumnMapping toCol = toMapping.Columns.Single(c => c.Member == fromCol.ReferenceTargetMember);
				if (!j.IsJoined)
				{
					writer.Indent()
						.NewLine().Append("JOIN ").Append(_dbObjectReference).Append(" AS ").Append(toRef)
						.Indent()
						.NewLine()
						.Append("ON ")
						.Append(toRef)
						.Append(mapping.QuoteObjectName(toCol.TargetName))
						.Append(" = ")
						.Append(fromRef).Append('.').Append(mapping.QuoteObjectName(fromCol.TargetName));
					Condition conditions = join.Conditions;
					if (conditions != null)
					{
						writer.Indent().NewLine().Append("AND ");
						conditions.WriteConditions(mapping, writer);
						writer.Outdent();
					}
					writer.Outdent().Outdent();
					j.IsJoined = true;
				}
				fromMapping = toMapping;
				fromRef = toRef;
			}
		}
	}
}