using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using System.Data.SqlTypes;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedDateTimeEmitter : SqlDbTypeEmitter<DateTime>
	{
		internal SqlMappedDateTimeEmitter(SqlDbType dbType)
			: base(default(DbType), dbType)
		{
			switch (dbType)
			{
				case SqlDbType.Date:
					this.DbType = DbType.Date;
					this.SpecializedSqlTypeName = "DATE";
					this.LengthRequirements = DbTypeLengthRequirements.None;
					break;
				case SqlDbType.DateTime:
					this.DbType = DbType.DateTime;
					this.LengthRequirements = DbTypeLengthRequirements.None;
					break;
				case SqlDbType.DateTime2:
					this.DbType = DbType.DateTime2;
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					this.TreatMissingLengthAsMaximum = true;
					this.LengthMaximum = "7";
					break;
				case SqlDbType.Time:
					this.DbType = DbType.Time;
					this.LengthRequirements = DbTypeLengthRequirements.None;
					break;
				default:
					throw new ArgumentOutOfRangeException("dbType");
			}		
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetDateTime", typeof(int));
		}

		/// <summary>
		///   Emits IL to translate the runtime type to the dbtype.
		/// </summary>
		/// <param name="il"></param>
		/// <remarks>
		///   At the time of the call the runtime value is on top of the stack.
		///   When the method returns the translated type must be on the top of the stack.
		/// </remarks>
		protected override void EmitTranslateRuntimeType(ILGenerator il)
		{
			il.NewObj(typeof(SqlDateTime).GetConstructor(new[] { typeof(DateTime) }));
			il.Box(typeof(SqlDateTime));
		}
	}

	internal class SqlMappedNullableDateTimeEmitter : SqlDbTypeEmitter<DateTime?>
	{
		internal SqlMappedNullableDateTimeEmitter(SqlDbType dbType)
			: base(default(DbType), dbType)
		{
			switch (dbType)
			{
				case SqlDbType.Date:
					this.DbType = DbType.Date;
					this.SpecializedSqlTypeName = "DATE";
					this.LengthRequirements = DbTypeLengthRequirements.None;
					break;
				case SqlDbType.DateTime:
					this.DbType = DbType.DateTime;
					this.LengthRequirements = DbTypeLengthRequirements.None;
					break;
				case SqlDbType.DateTime2:
					this.DbType = DbType.DateTime2;
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					this.TreatMissingLengthAsMaximum = true;
					this.LengthMaximum = "7";
					break;
				case SqlDbType.Time:
					this.DbType = DbType.Time;
					this.LengthRequirements = DbTypeLengthRequirements.None;
					break;
				default:
					throw new ArgumentOutOfRangeException("dbType");
			}		
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetDateTime", typeof(int));
			EmitTranslateRuntimeType(il);
		}

		/// <summary>
		///   Emits IL to translate the runtime type to the dbtype.
		/// </summary>
		/// <param name="il"></param>
		/// <remarks>
		///   At the time of the call the runtime value is on top of the stack.
		///   When the method returns the translated type must be on the top of the stack.
		/// </remarks>
		protected override void EmitTranslateRuntimeType(ILGenerator il)
		{
			il.NewObj(typeof(SqlDateTime).GetConstructor(new[] { typeof(DateTime) }));
			il.Box(typeof(SqlDateTime));
		}

		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(DateTime?).GetConstructor(new[] { typeof(DateTime) }));
		}
	}
}
