namespace FlitBit.Data.Expressions
{
	public class OrCondition : Condition
	{
		public OrCondition(Condition left, Condition right)
			: base(ConditionKind.OrElse)
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
		public override void WriteConditions(DbProviderHelper helper, SqlWriter builder)
		{
			this.Left.WriteConditions(helper, builder);
			builder.NewLine().Append("OR (")
				.Indent();
			this.Right.WriteConditions(helper, builder);
			builder.Append(")")
				.Outdent();
		}
	}
}