using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedTypeToStringEmitter: MappedAnyToStringEmitter<Type>
	{
	  public MappedTypeToStringEmitter(DbType dbType)
	    : base(dbType)
	  {
	    
	  }

		protected override void EmitTranslateType(MethodBuilder method)
		{
			var il = method.GetILGenerator();
			il.Call<Type>("GetType", BindingFlags.Static | BindingFlags.Public, typeof(string));
		}
	}
}