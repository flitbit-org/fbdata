using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
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
			cns.Conditions = handleBinaryExpression(binary, cns);
			foreach (var join in cns.Joins.Values.OrderBy(j => j.Ordinal))
			{
				var stack = new Stack<Tuple<Condition, bool>>();
				ProcessConditionsFor(join, cns.Conditions, stack);
			}
			return null;
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
						ProcessConditionsFor(join, ((AndCondition)condition).Left, path);
						ProcessConditionsFor(join, ((AndCondition)condition).Right, path);
					}
				}
			}
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
			var left = binary.Left as BinaryExpression;
			if (left == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Left.NodeType));
			}
			
			var right = binary.Right as BinaryExpression;
			if (right == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Right.NodeType));
			}

			return handleBinaryExpression(left, cns).Or(handleBinaryExpression(right, cns));
		}

		private Condition processAndAlso(BinaryExpression binary, Constraints cns)
		{
			var left = binary.Left as BinaryExpression;
			if (left == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Left.NodeType));
			}

			var right = binary.Right as BinaryExpression;
			if (right == null)
			{
				throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ", binary.Right.NodeType));
			}

			return handleBinaryExpression(left, cns).And(handleBinaryExpression(right, cns));
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
			return new ComparisonCondition(ConditionKind.Equal, lhs, rhs) 
			{
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

						var targetCol = j.Mapping.Columns.Single(c => c.Member == ((MemberExpression) expr).Member);

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

			public Dictionary<string, Join> Joins { get { return _joins; } }
			public Dictionary<string, Parameter> Parameters { get { return _parms; } }
			public Condition Conditions { get; set; }
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
			public Condition Conditions { get; set; }

			internal void AddCondition(Condition condition)
			{
				var existing = Conditions;
				if (existing == null)
				{
					Conditions = condition;
				}
				else
				{
					Conditions = existing.And(condition);
				}
			}
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
			public Condition(ConditionKind kind)
			{
				Kind = kind;
			}

			public ConditionKind Kind { get; private set; }

			public Condition Lift()
			{
				IsLifted = true;
				return this;
			}

			public bool IsLifted { get; private set; }

			internal virtual bool IsLiftCandidateFor(Join j)
			{
				// A condition is a lift candidate if its operands apply to the
				// joined type, or a join type with a lessor join ordinal, or self.
				return false;
			}

			public Condition And(Condition right)
			{
				return new AndCondition(this, right);
			}

			public Condition Or(Condition right)
			{
				return new OrCondition(this, right);
			}
		}

		class AndCondition : Condition
		{
			public AndCondition(Condition left, Condition right) : base(ConditionKind.AndAlso)
			{
				Left = left;
				Right = right;
			}

			public Condition Right { get; set; }

			public Condition Left { get; set; }

			internal override bool IsLiftCandidateFor(Join j)
			{
				return Left.IsLiftCandidateFor(j) && Right.IsLiftCandidateFor(j);
			}
		}

		class OrCondition : Condition
		{
			public OrCondition(Condition left, Condition right)
				: base(ConditionKind.OrElse)
			{
				Left = left;
				Right = right;
			}

			public Condition Right { get; set; }

			public Condition Left { get; set; }

			internal override bool IsLiftCandidateFor(Join j)
			{
				return Left.IsLiftCandidateFor(j) && Right.IsLiftCandidateFor(j);
			}
		}

		class ComparisonCondition : Condition
		{
			public ComparisonCondition(ConditionKind kind, ValueReference left, ValueReference right) 
				: base(kind)
			{
				Contract.Requires<ArgumentOutOfRangeException>(kind.HasFlag(ConditionKind.Comparison));
				Left = left;
				Right = right;
			}

			public ValueReference Left { get; protected set; }

			public ValueReference Right { get; protected set; }

			public string Text { get; set; }

			internal override bool IsLiftCandidateFor(Join j)
			{
				return !IsLifted
				       && Left.IsLiftCandidateFor(j)
				       && Right.IsLiftCandidateFor(j);
			}
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
			internal bool IsLiftCandidateFor(Join j)
			{
				var join = this.Join;
				return Join == null || Join.Ordinal <= j.Ordinal;
			}
		}
	}
}
