using System.Data.Common;
using System.Text;
using FlitBit.Data.Meta;
using System.Collections.Generic;
using System;

namespace FlitBit.Data
{
	public static class IModelBinderExtensions
	{
		public static void BuildDdlBatch(this IModelBinder binder, StringBuilder batch)
		{
			List<Type> members = new List<Type>();
			binder.BuildDdlBatch(batch, members);
		}
	}	
}
