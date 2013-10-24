using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
	internal class SqlDataModelCommandBuilder<TDataModel, TImpl, TCriteria> : IDataModelCommandBuilder<TDataModel, SqlConnection, TCriteria>
	{
		readonly Mapping<TDataModel> _mapping;

		public SqlDataModelCommandBuilder(Mapping<TDataModel> mapping)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			_mapping = mapping;
		}

		public IDataModelQueryManyCommand<TDataModel, SqlConnection, TCriteria> Where(Expression<Func<TDataModel, TCriteria, bool>> expression)
		{
			var sql = prepareTranslateExpression((expression.NodeType == ExpressionType.Lambda) ? expression.Body : expression);
			return null;
		}

		private string prepareTranslateExpression(Expression expr)
		{
			var cns = new Constraints();

			var binary = expr as BinaryExpression;
			if (binary == null) throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
			var condition = handleBinaryExpression(binary, cns);
			foreach (var join in cns.Joins.Values.OrderBy(j => j.Ordinal))
			{

			}
			condition.LeftReduce(null);
			return null;
		}

		private Condition handleBinaryExpression(BinaryExpression binary, Constraints cns)
		{
			if (binary.NodeType == ExpressionType.AndAlso)
			{
				return processAndAlso(binary, cns);
			}
			if (binary.NodeType == ExpressionType.OrElse)
			{
				return processOrElse(binary, cns);
			}
			if (binary.NodeType == ExpressionType.Equal)
			{
				return processEquals(binary, cns);
			}
			return default(Condition);
		}

		private Condition processOrElse(BinaryExpression binary, Constraints cns)
		{
			var res = new Condition { Kind = ConditionKind.OrElse };
			var left = binary.Left as BinaryExpression;
			if (left == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Left.NodeType));
			}
			Condition lhs = handleBinaryExpression(left, cns);
			if (lhs != null)
			{
				res.And(lhs);
			}

			var right = binary.Right as BinaryExpression;
			if (right == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Right.NodeType));
			}
			Condition rhs = handleBinaryExpression(right, cns);
			if (rhs != null)
			{
				res.Or(rhs);
			}
			return res;
		}

		private Condition processAndAlso(BinaryExpression binary, Constraints cns)
		{
			var res = new Condition { Kind = ConditionKind.AndAlso };
			var left = binary.Left as BinaryExpression;
			if (left == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Left.NodeType));
			}
			Condition lhs = handleBinaryExpression(left, cns);
			if (lhs != null)
			{
				res.And(lhs);
			}

			var right = binary.Right as BinaryExpression;
			if (right == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Right.NodeType));
			}
			Condition rhs = handleBinaryExpression(right, cns);
			if (rhs != null)
			{
				res.And(rhs);
			}
			return res;
		}

		ComparisonCondition processEquals(BinaryExpression binary, Constraints cns)
		{
			string res;
			var helper = _mapping.GetDbProviderHelper();
			var lhs = FormatValueReference(binary.Left, cns, helper);
			var rhs = FormatValueReference(binary.Right, cns, helper);
			if (lhs.Kind == ValueReferenceKind.Null)
			{
				res = rhs.Kind == ValueReferenceKind.Null ? String.Empty : String.Concat(rhs.Value, " IS NULL");
			}
			else
			{
				res = rhs.Kind == ValueReferenceKind.Null
					? String.Concat(lhs.Value, " IS NULL")
					: String.Concat(lhs.Value, " = ", rhs.Value);
			}
			return new ComparisonCondition()
			{
				Kind = ConditionKind.Equal,
				Left = lhs,
				Right = rhs,
				Text = res
			};
		}

		ValueReference FormatValueReference(Expression expr, Constraints cns, DbProviderHelper helper)
		{
			ValueReference res;
			if (expr.NodeType == ExpressionType.MemberAccess)
			{
				if (IsMemberOfSelf(expr))
				{
					var j = AddJoinPaths(expr, cns.Joins);
					if (j == null)
					{
						var col = _mapping.Columns.Single(c => c.Member == ((MemberExpression) expr).Member);
						res = new ValueReference
						{
							Kind = ValueReferenceKind.Self,
							Value = String.Concat(helper.QuoteObjectName("self"), '.',
								helper.QuoteObjectName(col.TargetName))
						};
					}
					else
					{

						var targetCol = Join.Mapping.Columns.Single(c => c.Member == ((MemberExpression) expr).Member);

						res = new ValueReference
						{
							Kind = ValueReferenceKind.Join,
							Join = j,
							Value = String.Concat(helper.QuoteObjectName(Convert.ToString(j.Ordinal)), '.',
								helper.QuoteObjectName(targetCol.TargetName))
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
				var c = (ConstantExpression)expr;
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

		private string AddParameter(MemberExpression expr, Dictionary<string, Parameter> parms)
		{
			var stack = new Stack<MemberExpression>();
			Expression it = expr;
			while (it.NodeType == ExpressionType.MemberAccess)
			{
				stack.Push((MemberExpression)it);
				it = ((MemberExpression)it).Expression;
			}
			if (it.NodeType != ExpressionType.Parameter)
				throw new NotSupportedException(String.Concat("Expression not supported as a parameter source: ", it.NodeType));
			var name = stack.Aggregate(((ParameterExpression)it).Name, (current, m) => String.Concat(current, '_', m.Member.Name));
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

		private Join AddJoinPaths(Expression expr, Dictionary<string, Join> joins)
		{
			var stack = new Stack<MemberExpression>();
			var it = ((MemberExpression)expr).Expression;
			while (it.NodeType == ExpressionType.MemberAccess)
			{
				stack.Push((MemberExpression)it);
				it = ((MemberExpression)it).Expression;
			}
			if (it.NodeType != ExpressionType.Parameter
					|| it.Type != typeof(TDataModel))
				throw new NotSupportedException();
			var s = "";
			var j = default(Join);
			foreach (var m in stack)
			{
				s = (s.Length > 0) ? String.Concat(s, '.', m.Member.Name) : m.Member.Name;
				if (!joins.TryGetValue(s, out j))
				{
					var targetType = m.Member.DeclaringType;
					var mapping = (targetType == typeof(TDataModel)) ? _mapping : Mappings.AccessMappingFor(targetType);
					joins.Add(s,
						j = new Join
						{
							Ordinal = joins.Count,
							Path = s,
							Mapping = mapping,
							TargetType = targetType
						});
				}
			}
			return j;
		}

		bool IsMemberOfSelf(Expression expr)
		{
			var it = expr;
			while (it.NodeType == ExpressionType.MemberAccess)
			{
				it = ((MemberExpression)it).Expression;
			}
			if (it.NodeType == ExpressionType.Parameter
					&& it.Type == typeof(TDataModel))
			{
				return true;
			}
			return false;
		}

		class Constraints
		{
			readonly Dictionary<string, Join> _joins = new Dictionary<string, Join>();
			readonly Dictionary<string, Parameter> _parms = new Dictionary<string, Parameter>();
			readonly Condition _conditions = new Condition {Kind = ConditionKind.AndAlso};

			public Dictionary<string, Join> Joins { get { return _joins; } }
			public Dictionary<string, Parameter> Parameters { get { return _parms; } }
			public Condition Conditions { get { return _conditions; } }
		}

		class Parameter
		{
			public MemberExpression Member { get; set; }
			public string Name { get; set; }
		}

		class Join
		{
			public int Ordinal { get; set; }
			public string Path { get; set; }
			public Type TargetType { get; set; }

			public IMapping Mapping { get; set; }
		}

		[Flags]
		enum ConditionKind
		{
			Unknown = 0,
			AndAlso = 1,
			OrElse = 1 << 1,
			Comparison = 1 << 2,
			Equal = Comparison | 1 << 3,
			NotEqual = Comparison | 1 << 4,
		}

		class Condition
		{
			readonly Stack<Condition> _stack = new Stack<Condition>();
			readonly List<Condition> _leftAndConditions = new List<Condition>();
			readonly List<Condition> _orElseConditions = new List<Condition>();

			public ConditionKind Kind { get; set; }

			public void And(Condition cond)
			{
				if (_stack.Count > 0)
				{
					_stack.Peek().And(cond);
				}
				else
				{
					_leftAndConditions.Add(cond);
					var self = this;
					cond.End = () => self;
				}
			}

			public void Or(Condition cond)
			{
				if (_stack.Count > 0)
				{
					_stack.Peek().Or(cond);
				}
				else
				{
					_orElseConditions.Add(cond);
					_stack.Push(cond);
					var self = this;
					cond.End = () =>
					{
						_stack.Pop();
						return self;
					};
				}
			}

			public void LeftReduce(Func<Condition, bool> lift)
			{
				var len = _leftAndConditions.Count;
				var i = -1;
				while (++i < len)
				{
					if (_leftAndConditions[i].Kind.HasFlag(ConditionKind.Comparison))
					{
						var c = (ComparisonCondition)_leftAndConditions[i];
						if (c.Left.Kind.HasFlag(ValueReferenceKind.Join))
						{
						}
					}
				}
			}
			
			public Func<Condition> End { get; private set; }
		}

		class ComparisonCondition : Condition
		{
			public ValueReference Left { get; set; }

			public ValueReference Right { get; set; }

			public string Text { get; set; }
		}

		[Flags]
		enum ValueReferenceKind
		{
			Unknown = 0,
			Ref = 1,
			Self = Ref | 1 << 1,
			Join = Ref | 1 << 2,
			Param = 1 << 3,
			Constant = 1 << 4,
			Null = Constant | 1 << 5
		}

		class ValueReference
		{
			public ValueReferenceKind Kind { get; set; }
			public string Value { get; set; }
			public Join Join { get; set; }
		}
	}
}
