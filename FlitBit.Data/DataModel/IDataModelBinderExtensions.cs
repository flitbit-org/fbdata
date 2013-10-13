using System;
using System.Collections.Generic;
using System.Text;

namespace FlitBit.Data.DataModel
{
	public static class IDataModelBinderExtensions
	{
		public static void BuildDdlBatch(this IDataModelBinder binder, StringBuilder batch)
		{
			var members = new List<Type>();
			binder.BuildDdlBatch(batch, members);
		}
	}
}