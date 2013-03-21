﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FlitBit.Data
{
	public static class IModelBinderExtensions
	{
		public static void BuildDdlBatch(this IModelBinder binder, StringBuilder batch)
		{
			var members = new List<Type>();
			binder.BuildDdlBatch(batch, members);
		}
	}
}