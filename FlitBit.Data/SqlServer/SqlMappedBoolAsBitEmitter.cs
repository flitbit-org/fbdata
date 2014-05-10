#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using FlitBit.Emit;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedBoolAsBitEmitter : SqlDbTypeEmitter<bool>
	{
		internal SqlMappedBoolAsBitEmitter()
			: base(DbType.Boolean, SqlDbType.Bit)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetBoolean", typeof(int));
		}

		protected override string TransformConstantValueToString(object value)
		{
			return (bool) value ? "1" : "0";
		}
	}
}