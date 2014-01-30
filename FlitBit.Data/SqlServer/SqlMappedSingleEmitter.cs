﻿using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedSingleEmitter : SqlDbTypeEmitter<float>
  {
    internal SqlMappedSingleEmitter()
      : base(DbType.Single, SqlDbType.Real)
    {
    }
    public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
    {
      var il = method.GetILGenerator();
      reader.LoadValue(il);
      columnIndex.LoadValue(il);
      il.CallVirtual<DbDataReader>("GetFloat", typeof(int));
    }
    public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping, ColumnMapping<TModel> col)
    {
    }

    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
      il.NewObj(typeof(SqlSingle).GetConstructor(new[] { typeof(float) }));
      il.Box(typeof(SqlSingle));
    }
  }
}