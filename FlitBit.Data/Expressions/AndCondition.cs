using FlitBit.Data.DataModel;

namespace FlitBit.Data.Expressions
{
	public class AndCondition : Condition
	{
		public AndCondition(Condition left, Condition right) : base(ConditionKind.AndAlso)
		{
			this.Left = left;
			this.Right = right;
		}

		public Condition Right { get; set; }

		public Condition Left { get; set; }

		internal override bool IsLiftCandidateFor(Join j)
		{
			return this.Left.IsLiftCandidateFor(j) && this.Right.IsLiftCandidateFor(j);
		}

		public override void WriteConditions(IMapping mapping, SqlWriter builder)
		{
			this.Left.WriteConditions(mapping, builder);
			builder.NewLine().Append("AND ");
			this.Right.WriteConditions(mapping, builder);
		}
	}
}