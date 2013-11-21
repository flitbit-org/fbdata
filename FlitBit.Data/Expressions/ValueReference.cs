using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
	public class ValueReference
	{
		public ValueReference(ValueReferenceKind kind)
		{
			Kind = kind;
		}
		public ValueReferenceKind Kind { get; private set; }

		public string Value { get; set; }

		public Join Join { get; set; }

		internal bool IsLiftCandidateFor(Join j)
		{
			var join = this.Join;
			return join == null || join.Ordinal <= j.Ordinal;
		}

		public ColumnMapping Column { get; private set; }

		public Parameter Parameter { get; set; }

		public ValueReference AssociateColumn(ColumnMapping col)
		{
			if (Column == null)
			{
				Column = col;
				if (Parameter != null && Parameter.Column == null)
				{
					Parameter.Column = col;
				}
			}
			return this;
		}
	}
}