#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedStringEmitter : SqlMappedAnyToStringEmitter<string>
  {
    internal SqlMappedStringEmitter(SqlDbType dbType)
      : base(dbType)
    {
      this.IsQuoteRequired = true;
      this.QuoteChars = "'";
      this.DelimitedQuoteChars = "''";
      this.MissingLengthBindValue = -1;
    }
  }
}