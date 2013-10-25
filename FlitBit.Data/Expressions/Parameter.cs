using System.Linq.Expressions;

namespace FlitBit.Data.Expressions
{
	public class Parameter
	{
		public MemberExpression Member { get; set; }
		public string Name { get; set; }
	}
}