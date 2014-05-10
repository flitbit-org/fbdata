#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedEmumAsInt16Emitter<TEnum> : SqlDbTypeEmitter<TEnum>
    where TEnum : struct
  {
    internal SqlMappedEmumAsInt16Emitter()
      : base(DbType.Int16, SqlDbType.SmallInt, typeof(short)) { DbDataReaderGetValueMethodName = "GetInt16"; }

    //protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    //{
    //  il.LoadLocal(local);
    //  il.NewObj(typeof(SqlInt16).GetConstructor(new[] { typeof(short) }));
    //  il.Box(typeof(SqlInt16));
    //}
  }
}