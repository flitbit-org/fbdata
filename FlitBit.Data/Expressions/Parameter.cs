using System.Reflection;

namespace FlitBit.Data.Expressions
{
	public class Parameter
	{
		public string Name { get; set; }

		public ParameterValueReference Argument { get; set; }

		public MemberInfo[] Members { get; set; }
	}
}