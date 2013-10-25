using System.Reflection;

namespace FlitBit.Data.Expressions
{
	public class ValueReference
	{
		public ValueReferenceKind Kind { get; set; }
		public string Value { get; set; }
		public Join Join { get; set; }
		internal bool IsLiftCandidateFor(Join j)
		{
			var join = this.Join;
			return join == null || join.Ordinal <= j.Ordinal;
		}

		public MemberInfo Member { get; set; }
	}
}