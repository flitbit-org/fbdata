using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
	public class DataModelSqlWriter<TDataModel>
	{
		public static readonly string DefaultSelfName = "self";
		public static readonly string DefaultIndent = "  ";
		readonly Mapping<TDataModel> _mapping = Mapping<TDataModel>.Instance;
		readonly string _indent;
		readonly int _bufferLength;
		readonly string _selfRef;

		public DataModelSqlWriter()
			: this(DefaultSelfName, DefaultIndent)
		{
		}

		public DataModelSqlWriter(string selfName, string indent)
		{
			Contract.Requires<ArgumentNullException>(selfName != null);
			Contract.Requires<ArgumentException>(selfName.Length > 0);
			Contract.Requires<ArgumentNullException>(indent != null);
			_selfRef = Mapping.QuoteObjectNameForSQL(selfName);
			_indent = indent;
			_bufferLength = _mapping.Columns.Count()*40;
		} 

		public IMapping<TDataModel> Mapping { get { return _mapping; } }

		public string Select
		{
			get
			{
				var mapping = Mapping;
				var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
				writer.Append("SELECT ");
				AppendColumns(writer, mapping.Columns.OrderBy(c => c.Ordinal), c => String.Concat(_selfRef, ".", mapping.QuoteObjectNameForSQL(c.TargetName)));
				writer.Outdent().NewLine()
					.Append("FROM ").Append(mapping.DbObjectReference)
					.Append(" AS ").Append(_selfRef);
				return writer.ToString();
			}
		}

		public string SelectInPrimaryKeyOrder
		{
			get
			{
				var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent)
					.Append(Select);
				AppendOrderByPrimaryKey(writer);
				return writer.ToString();
			}
		}

		public string SelectInPrimaryKeyOrderWithPaging
		{
			get
			{
				return WriteSelectWithPaging(null, null);
			}
		}

		string WriteSelectWithPaging(Constraints cns, IEnumerable<Tuple<ColumnMapping, bool>> orderBy)
		{
			var mapping = Mapping;
			// Default to PK order...
			var order = (orderBy ?? mapping.Identity.Columns.Select(c => Tuple.Create(c, false))).ToArray();
			var writer = new SqlWriter(_bufferLength, Environment.NewLine, _indent);
			
			writer.Append("DECLARE @endRow INT = @startRow + (@pageSize - 1);")
				.NewLine("WITH dataset AS(").Indent()
				.NewLine("SELECT ");
			AppendColumns(writer, mapping.Columns.OrderBy(c => c.Ordinal), c => String.Concat(_selfRef, ".", mapping.QuoteObjectNameForSQL(c.TargetName)));
			writer.NewLine("ROW_NUMBER() OVER(ORDER BY ");
			var i = 0;
			var orderByStatement = new StringBuilder();
			var orderByStatementRev = new StringBuilder();
			foreach (var o in order)
			{
				if (i++ > 0)
				{
					orderByStatement.Append(", ").Append(_selfRef).Append('.').Append(o.Item1.TargetName);
					orderByStatement.Append(", ").Append(_selfRef).Append('.').Append(o.Item1.TargetName);
				}
				else
				{
					orderByStatement.Append(_selfRef).Append('.').Append(o.Item1.TargetName);
					orderByStatement.Append(_selfRef).Append('.').Append(o.Item1.TargetName);
				}
				orderByStatement.Append(o.Item2 ? " ASC": " DESC");
				orderByStatement.Append(o.Item2 ? " DESC": " ASC");
			}

			writer.Append(orderByStatement.ToString()).Append(") AS seq,")
				.NewLine("ROW_NUMBER() OVER(ORDER BY ")
				.Append(orderByStatementRev.ToString())
				.Append(") AS rev_seq,")
				.Outdent().NewLine("FROM ").Append(mapping.DbObjectReference).Append(" AS ").Append(_selfRef)
				.NewLine(")")
				.NewLine(@"SELECT TOP (@pageSize) ");
			AppendColumns(writer, mapping.Columns.OrderBy(c => c.Ordinal), c => String.Concat(_selfRef, ".", mapping.QuoteObjectNameForSQL(c.TargetName)));
			writer.Append(",").NewLine("rev_seq + seq -1 as TotalRows")
				.Outdent().NewLine("FROM dataset AS [self]")
				.NewLine("WHERE seq >= @startRow")
				.NewLine("ORDER BY seq");

			return writer.ToString();
		}

		private void AppendColumns(SqlWriter writer, IEnumerable<ColumnMapping> columns, Func<ColumnMapping, string> eaColumn)
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

		void AppendOrderByPrimaryKey(SqlWriter writer)
		{
			var mapping = Mapping;
			writer.NewLine().Append("ORDER BY ");
			var i = 0;
			foreach (var c in mapping.Identity.Columns)
			{
				if (i++ > 0)
				{
					writer.Append(',').NewLine().Append(_selfRef).Append('.').Append(c.TargetName);
				}
				else
				{
					writer.Append(_selfRef).Append('.').Append(c.TargetName);
					writer.Indent();
				}
			}
			writer.Outdent();
		}

		public void PrepareFromAndWhereStatement(Constraints cns, SqlWriter writer)
		{
			// Perform necessary joins and write join clauses...
			foreach (var join in cns.Joins.Values.OrderBy(j => j.Ordinal))
			{
				var stack = new Stack<Tuple<Condition, bool>>();
				ProcessConditionsFor(join, cns.Conditions, stack);
				MapJoinFrom(join, cns);
			}
			// Write the primary statement's conditions...
			var c = cns.Conditions;
			if (c != null && !c.IsLifted)
			{
				writer.NewLine().Indent().Append("WHERE ");
				c.WriteConditions(Mapping<TDataModel>.Instance, writer);
				writer.Outdent();
			}
		}

		void ProcessConditionsFor(Join join, Condition condition, Stack<Tuple<Condition, bool>> path)
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
						ProcessConditionsFor(join, ((AndCondition)condition).Left, path);
						ProcessConditionsFor(join, ((AndCondition)condition).Right, path);
					}
				}
			}
		}

		void MapJoinFrom(Join join, Constraints cns)
		{
			var writer = cns.Writer;
			var jj = join;
			var joins = new Stack<Join>();
			while (jj != null)
			{
				joins.Push(jj);
				jj = jj.Path;
			}
			var mapping = Mapping<TDataModel>.Instance;
			IMapping fromMapping = mapping;
			var fromRef = mapping.QuoteObjectNameForSQL("self");
			foreach (var j in joins)
			{
				var toMapping = j.Mapping;
				var fromCol = fromMapping.Columns.Single(c => c.Member == j.Member);
				var toRef = mapping.QuoteObjectNameForSQL(Convert.ToString(j.Ordinal));
				var toCol = toMapping.Columns.Single(c => c.Member == fromCol.ReferenceTargetMember);
				if (!j.IsJoined)
				{
					writer.Indent()
						.NewLine().Append("JOIN ").Append(toMapping.DbObjectReference).Append(" AS ").Append(toRef)
						.Indent().NewLine().Append("ON ").Append(toRef).Append(mapping.QuoteObjectNameForSQL(toCol.TargetName)).Append(" = ")
						.Append(fromRef).Append('.').Append(mapping.QuoteObjectNameForSQL(fromCol.TargetName));
					var conditions = join.Conditions;
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