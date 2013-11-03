using FlitBit.Data.DataModel;

namespace FlitBit.Data.Expressions
{
	public class Condition
	{
		public Condition(ConditionKind kind)
		{
			this.Kind = kind;
		}

		public ConditionKind Kind { get; private set; }

		public Condition Lift()
		{
			this.IsLifted = true;
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

		public virtual void WriteConditions(IMapping mapping, SqlWriter builder)
		{
		}
	}
}