using System.Reflection;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Expressions
{
	public class Join
	{

		public int Ordinal { get; set; }

		public string Key { get; set; }

		public Join Path { get; set; }

    public IMapping Mapping { get; set; }

    public IMapping ToMapping { get; set; }
    
		public Condition Conditions { get; set; }

		internal void AddCondition(Condition condition)
		{
			var lifted = condition.Lift();
			var existing = this.Conditions;
			this.Conditions = existing == null ? lifted : existing.And(lifted);
		}

		public bool IsJoined { get; set; }
	}
}