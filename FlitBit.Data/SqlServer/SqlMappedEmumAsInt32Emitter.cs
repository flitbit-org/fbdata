#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedEmumAsInt32Emitter<TEnum> : SqlDbTypeEmitter<TEnum>
    where TEnum : struct
  {
    internal SqlMappedEmumAsInt32Emitter()
      : base(DbType.Int16, SqlDbType.Int, typeof(int)) { DbDataReaderGetValueMethodName = "GetInt32"; }

    //protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    //{
    //  il.LoadLocal(local);
    //  il.NewObj(typeof(SqlInt32).GetConstructor(new[] { typeof(int) }));
    //  il.Box(typeof(SqlInt32));
    //}
  }
}