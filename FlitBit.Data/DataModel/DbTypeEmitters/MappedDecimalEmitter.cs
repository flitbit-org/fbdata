#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedDecimalEmitter : MappedDbTypeEmitter<decimal, DbType>
	{
		internal MappedDecimalEmitter()
			: base(DbType.Decimal, DbType.Decimal)
		{
      this.LengthRequirements = DbTypeLengthRequirements.Precision | DbTypeLengthRequirements.Scale | DbTypeLengthRequirements.OptionalScale;

		  DbDataReaderGetValueMethodName = "GetDecimal";
		}

	}
}