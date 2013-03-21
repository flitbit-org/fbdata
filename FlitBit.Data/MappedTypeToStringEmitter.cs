using System;
using System.Data;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data
{
	internal class MappedTypeToStringEmitter: MappedAnyToStringEmitter<TypeBuilder>
	{
		public MappedTypeToStringEmitter(DbType dbType)
			: base(dbType)
		{}

		protected override void EmitTranslateType(MethodBuilder method)
		{
			var il = method.GetILGenerator();
			il.New<Type>(typeof(string));
		}
	}
}