using System.Reflection;

namespace FlitBit.Data
{
	public interface IDbTypeTranslator
	{
		DbTypeTranslation GetTranslation(MemberInfo member);
	}
}