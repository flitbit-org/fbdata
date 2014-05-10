#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.Core;

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
          source.GetType()
                .GetReadableFullName())
        );
    }
  }
}