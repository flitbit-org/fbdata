#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion
										
using System;
using FlitBit.Core;
using FlitBit.Emit;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	public static class Conversion
	{
		public static Guid ToGuid(object source)
		{
			Contract.Requires<ArgumentNullException>(source != null);

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
			throw new ArgumentException(
				String.Concat("Cannot convert type to Guid: ", 
				source.GetType().GetReadableFullName())
				);
		}
	}
}
