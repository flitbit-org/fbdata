using System;
using System.Linq.Expressions;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Expressions
{
	public class ComparisonCondition : Condition
	{
		public ComparisonCondition(ExpressionType exprType, ValueReference left, ValueReference right) 
			: base(ConditionKind.Comparison)
		{
			this.ExprType = exprType;
			this.Left = left;
			this.Right = right;
		}

		public ExpressionType ExprType { get; private set; }

		public ValueReference Left { get; protected set; }

		public ValueReference Right { get; protected set; }

		public string Text { get; set; }

		internal override bool IsLiftCandidateFor(Join j)
		{
			return !this.IsLifted
						&& this.Left.IsLiftCandidateFor(j)
						&& this.Right.IsLiftCandidateFor(j);
		}

		public override void WriteConditions(IMapping mapping, SqlWriter builder)
		{
			if (String.IsNullOrEmpty(this.Text))
			{
				var helper = mapping.GetDbProviderHelper();
				builder.Append(helper.TranslateComparison(this.ExprType, this.Left, this.Right));
			}
			else
			{
				builder.Append(this.Text);
			}
		}
	}
}