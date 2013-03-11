using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FlitBit.Data
{
	public interface IDbTypeTranslator
	{
		DbTypeTranslation GetTranslation(MemberInfo member);
	}
}
