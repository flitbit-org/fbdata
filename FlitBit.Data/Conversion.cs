#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.Data
{
	public static class Conversion
	{
		public static Guid ToGuid(object source)
		{
			if (typeof(Guid).IsInstanceOfType(source))
			{
				return (Guid)source;
			}
			if (typeof(String).IsInstanceOfType(source))
			{
				Guid result;
				if (Guid.TryParse((string)source, out result))
				{
					return result;
				}
			}
			throw new NotImplementedException(
				String.Concat("Cannot convert type to Guid: ", 
				source.GetType().GetReadableFullName())
				);
		}
	}
}
