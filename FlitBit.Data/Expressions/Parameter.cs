using System.Reflection;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
	public class Parameter
	{
		public string Name { get; set; }

		public ParameterValueReference Argument { get; set; }

		public MemberInfo[] Members { get; set; }

		public ColumnMapping Column { get; set; }

    public int Ordinal { get; set; }
  }
}