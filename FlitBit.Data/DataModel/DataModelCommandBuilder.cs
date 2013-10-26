using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using FlitBit.Core;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SqlServer;

namespace FlitBit.Data.DataModel
{
	public abstract class DataModelCommandBuilder<TDataModel, TDbConnection, TCriteria> :
		IDataModelCommandBuilder<TDataModel, TDbConnection, TCriteria>
	{
		readonly DataModelSqlWriter<TDataModel> _sqlWriter;

		public DataModelCommandBuilder(DataModelSqlWriter<TDataModel> sqlWriter)
		{
			Contract.Requires<ArgumentNullException>(sqlWriter != null);
			_sqlWriter = sqlWriter;
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TCriteria> Where(
			Expression<Func<TDataModel, TCriteria, bool>> expression)
		{
			return
				ConstructCommandOnConstraints(
																		 PrepareTranslateExpression((expression.NodeType == ExpressionType.Lambda)
																			 ? expression.Body
																			 : expression));
		}

		protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TCriteria> ConstructCommandOnConstraints(
			Constraints constraints);

		Constraints PrepareTranslateExpression(Expression expr)
		{
			var cns = new Constraints();
			cns.Writer.Append(_sqlWriter.Select);

			var binary = expr as BinaryExpression;
			if (binary == null) throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
			cns.Conditions = HandleBinaryExpression(binary, cns);
			_sqlWriter.PrepareFromAndWhereStatement(cns, cns.Writer);
			return cns;
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
			var lhs = FormatValueReference(binary.Left, cns);
			var rhs = FormatValueReference(binary.Right, cns);
			return new ComparisonCondition(binary.NodeType, lhs, rhs);
		}

		ValueReference FormatValueReference(Expression expr, Constraints cns)
		{
			var mapping = Mapping<TDataModel>.Instance;
			var helper = mapping.GetDbProviderHelper();
			ValueReference res;
			if (expr.NodeType == ExpressionType.MemberAccess)
			{
				var m = ((MemberExpression) expr).Member;
				if (IsMemberOfSelf(expr))
				{
					var j = AddJoinPaths(expr, cns.Joins);
					if (j == null)
					{
						var col = mapping.Columns.Single(c => c.Member == m);
						res = new ValueReference
						{
							Kind = ValueReferenceKind.Self,
							Value = String.Concat(mapping.QuoteObjectNameForSQL("self"), '.',
								mapping.QuoteObjectNameForSQL(col.TargetName))
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
								String.Concat(mapping.QuoteObjectNameForSQL(Convert.ToString(j.Ordinal)), ".",
									mapping.QuoteObjectNameForSQL(foreignColumn.TargetName))
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
					var emitter = helper.GetDbTypeEmitter(mapping, expr.Type);
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
			IMapping fromMapping = Mapping<TDataModel>.Instance;
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