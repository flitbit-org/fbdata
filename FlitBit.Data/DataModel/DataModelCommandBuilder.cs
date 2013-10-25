using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using FlitBit.Core;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
	public abstract class DataModelCommandBuilder<TDataModel, TDbConnection, TCriteria> :
		IDataModelCommandBuilder<TDataModel, TDbConnection, TCriteria>
	{
		readonly Mapping<TDataModel> _mapping;
		readonly string _select;

		public DataModelCommandBuilder(Mapping<TDataModel> mapping, string select)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentNullException>(select != null);
			Contract.Requires<ArgumentException>(select.Length > 0);
			_mapping = mapping;
			_select = select;
		}

		public IDataModelQueryManyCommand<TDataModel, TDbConnection, TCriteria> Where(
			Expression<Func<TDataModel, TCriteria, bool>> expression)
		{
			return
				ConstructCommandOnConstraints(
																		 PrepareTranslateExpression((expression.NodeType == ExpressionType.Lambda)
																			 ? expression.Body
																			 : expression));
		}

		protected abstract IDataModelQueryManyCommand<TDataModel, TDbConnection, TCriteria> ConstructCommandOnConstraints(
			Constraints constraints);

		Constraints PrepareTranslateExpression(Expression expr)
		{
			var cns = new Constraints();
			cns.Writer.Append(_select);

			var binary = expr as BinaryExpression;
			if (binary == null) throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
			cns.Conditions = HandleBinaryExpression(binary, cns);
			// Perform necessary joins and write join clauses...
			foreach (var join in cns.Joins.Values.OrderBy(j => j.Ordinal))
			{
				var stack = new Stack<Tuple<Condition, bool>>();
				ProcessConditionsFor(join, cns.Conditions, stack);
				MapJoinFrom(_mapping, join, cns);
			}
			// Write the primary statement's conditions...
			PrepareWhereStatement(cns);
			return cns;
		}

		private void PrepareWhereStatement(Constraints cns)
		{
			var helper = _mapping.GetDbProviderHelper();
			var writer = cns.Writer; 
			var c = cns.Conditions;
			if (c != null && !c.IsLifted)
			{
				writer.NewLine().Indent().Append("WHERE ");
				c.WriteConditions(helper, writer);
				writer.Outdent();
			}
		}

		void MapJoinFrom(IMapping mapping, Join join, Constraints cns)
		{
			var writer = cns.Writer;
			var jj = join;
			var joins = new Stack<Join>();
			while (jj != null)
			{
				joins.Push(jj);
				jj = jj.Path;
			}
			var helper = _mapping.GetDbProviderHelper();
			IMapping fromMapping = mapping;
			var fromRef = helper.QuoteObjectName("self");
			foreach (var j in joins)
			{
				var toMapping = j.Mapping;
				var fromCol = fromMapping.Columns.Single(c => c.Member == j.Member);
				var toRef = helper.QuoteObjectName(Convert.ToString(j.Ordinal));
				var toCol = toMapping.Columns.Single(c => c.Member == fromCol.ReferenceTargetMember);
				if (!j.IsJoined)
				{
					writer.Indent()
						.NewLine().Append("JOIN ").Append(toMapping.DbObjectReference).Append(" AS ").Append(toRef)
						.Indent().NewLine().Append("ON ").Append(toRef).Append(helper.QuoteObjectName(toCol.TargetName)).Append(" = ")
						.Append(fromRef).Append('.').Append(helper.QuoteObjectName(fromCol.TargetName));
					var conditions = join.Conditions;
					if (conditions != null)
					{
						writer.Indent().NewLine().Append("AND ");
						conditions.WriteConditions(helper, writer);
						writer.Outdent();
					}
					writer.Outdent().Outdent();
					j.IsJoined = true;
				}
				fromMapping = toMapping;
				fromRef = toRef;
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
						ProcessConditionsFor(join, ((AndCondition) condition).Left, path);
						ProcessConditionsFor(join, ((AndCondition) condition).Right, path);
					}
				}
			}
		}

		Condition HandleBinaryExpression(BinaryExpression binary, Constraints cns)
		{
			if (binary.NodeType == ExpressionType.AndAlso)
			{
				return ProcessAndAlso(binary, cns);
			}
			if (binary.NodeType == ExpressionType.OrElse)
			{
				return ProcessOrElse(binary, cns);
			}
			if (binary.NodeType == ExpressionType.Equal)
			{
				return ProcessComparison(binary, cns);
			}
			if (binary.NodeType == ExpressionType.NotEqual)
			{
				return ProcessComparison(binary, cns);
			}
			return default(Condition);
		}

		Condition ProcessOrElse(BinaryExpression binary, Constraints cns)
		{
			var left = binary.Left as BinaryExpression;
			if (left == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
					binary.Left.NodeType));
			}

			var right = binary.Right as BinaryExpression;
			if (right == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
					binary.Right.NodeType));
			}

			return HandleBinaryExpression(left, cns).Or(HandleBinaryExpression(right, cns));
		}

		Condition ProcessAndAlso(BinaryExpression binary, Constraints cns)
		{
			var left = binary.Left as BinaryExpression;
			if (left == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
					binary.Left.NodeType));
			}

			var right = binary.Right as BinaryExpression;
			if (right == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
					binary.Right.NodeType));
			}

			return HandleBinaryExpression(left, cns).And(HandleBinaryExpression(right, cns));
		}

		ComparisonCondition ProcessComparison(BinaryExpression binary, Constraints cns)
		{
			var helper = _mapping.GetDbProviderHelper();
			var lhs = FormatValueReference(binary.Left, cns, helper);
			var rhs = FormatValueReference(binary.Right, cns, helper);
			return new ComparisonCondition(binary.NodeType, lhs, rhs);
		}

		ValueReference FormatValueReference(Expression expr, Constraints cns, DbProviderHelper helper)
		{
			ValueReference res;
			if (expr.NodeType == ExpressionType.MemberAccess)
			{
				var m = ((MemberExpression) expr).Member;
				if (IsMemberOfSelf(expr))
				{
					var j = AddJoinPaths(expr, cns.Joins);
					if (j == null)
					{
						var col = _mapping.Columns.Single(c => c.Member == m);
						res = new ValueReference
						{
							Kind = ValueReferenceKind.Self,
							Value = String.Concat(helper.QuoteObjectName("self"), '.',
								helper.QuoteObjectName(col.TargetName))
						};
					}
					else
					{
						var foreignColumn = j.Mapping.Columns.Single(c => c.Member == m);
						res = new ValueReference
						{
							Kind = ValueReferenceKind.Join,
							Join = j,
							Member = m,
							Value =
								String.Concat(helper.QuoteObjectName(Convert.ToString(j.Ordinal)), ".",
									helper.QuoteObjectName(foreignColumn.TargetName))
						};
					}
				}
				else
				{
					res = new ValueReference
					{
						Kind = ValueReferenceKind.Param,
						Value = helper.FormatParameterName(AddParameter((MemberExpression) expr, cns.Parameters))
					};
				}
			}
			else if (expr.NodeType == ExpressionType.Constant)
			{
				var c = (ConstantExpression) expr;
				if (c.Type.IsClass && c.Value == null)
				{
					res = new ValueReference
					{
						Kind = ValueReferenceKind.Null
					};
				}
				else
				{
					var emitter = helper.GetDbTypeEmitter(_mapping, expr.Type);
					if (emitter == null)
					{
						throw new NotSupportedException(String.Concat("No emitter found for type: ", expr.Type));
					}
					res = new ValueReference
					{
						Kind = ValueReferenceKind.Constant,
						Value = emitter.PrepareConstantValueForSql(c.Value)
					};
				}
			}
			else
			{
				throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
			}
			return res;
		}

		string AddParameter(MemberExpression expr, Dictionary<string, Parameter> parms)
		{
			var stack = new Stack<MemberExpression>();
			Expression it = expr;
			while (it.NodeType == ExpressionType.MemberAccess)
			{
				stack.Push((MemberExpression) it);
				it = ((MemberExpression) it).Expression;
			}
			if (it.NodeType != ExpressionType.Parameter)
				throw new NotSupportedException(String.Concat("Expression not supported as a parameter source: ", it.NodeType));
			var name = stack.Aggregate(((ParameterExpression) it).Name,
				(current, m) => String.Concat(current, '_', m.Member.Name));
			Parameter p;
			if (!parms.TryGetValue(name, out p))
			{
				parms.Add(name, new Parameter()
				{
					Name = name,
					Member = expr
				});
			}
			return name;
		}

		Join AddJoinPaths(Expression expr, Dictionary<string, Join> joins)
		{
			var stack = new Stack<MemberExpression>();
			var item = ((MemberExpression) expr).Expression;
			while (item.NodeType == ExpressionType.MemberAccess)
			{
				stack.Push((MemberExpression) item);
				item = ((MemberExpression) item).Expression;
			}
			if (item.NodeType != ExpressionType.Parameter
					|| item.Type != typeof(TDataModel))
				throw new NotSupportedException();
			var key = "";
			var outer = default(Join);
			IMapping fromMapping = _mapping;
			foreach (var it in stack)
			{
				var m = it.Member;
				key = (key.Length > 0) ? String.Concat(key, '.', m.Name) : m.Name;
				Join j;
				if (!joins.TryGetValue(key, out j))
				{
					var toDep = fromMapping.Dependencies.SingleOrDefault(d => d.Member == m);
					if (toDep == null)
					{
						throw new NotSupportedException(String.Concat("A dependency path is not known from ",
							fromMapping.RuntimeType.GetReadableSimpleName(),
							" to ", m.DeclaringType.GetReadableSimpleName(), " via the property `", m.Name, "'."));
					}
					joins.Add(key,
						j = new Join
						{
							Ordinal = joins.Count,
							Key = key,
							Path = outer,
							Mapping = toDep.Target,
							Member = m
						});
				}
				fromMapping = j.Mapping;
				outer = j;
			}

			return outer;
		}

		bool IsMemberOfSelf(Expression expr)
		{
			var it = expr;
			while (it.NodeType == ExpressionType.MemberAccess)
			{
				it = ((MemberExpression) it).Expression;
			}
			return it.NodeType == ExpressionType.Parameter
						&& it.Type == typeof(TDataModel);
		}

	}
}