#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core.Properties;

namespace FlitBit.Data.SqlServer
{
	internal static class SqlDbTypeTranslations
	{
		static readonly DbTypeTranslation[] DbTypeMap = new DbTypeTranslation[]
			{ new SqlDbTypeTranslation(DbType.AnsiString, SqlDbType.VarChar, typeof(string), false, "VARCHAR", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "MAX")
			, new SqlDbTypeTranslation(DbType.Binary, SqlDbType.Binary, typeof(byte[]), true, "VARBINARY", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "MAX")
			, new SqlDbTypeTranslation(DbType.Byte, SqlDbType.TinyInt, typeof(byte), true, "TINYINT", DbTypeLengthRequirements.None)
			, new SqlDbTypeTranslation(DbType.Boolean, SqlDbType.Bit, typeof(bool), true, "BIT", DbTypeLengthRequirements.None)
			, new SqlDbTypeTranslation(DbType.Currency, SqlDbType.Money, typeof(decimal), false, "MONEY", DbTypeLengthRequirements.None)
			, new SqlDbTypeTranslation(DbType.Date, SqlDbType.Date, typeof(DateTime), false, "DATE", DbTypeLengthRequirements.None)
			, new SqlDbTypeTranslation(DbType.DateTime, SqlDbType.DateTime, typeof(DateTime), true)
			, new SqlDbTypeTranslation(DbType.Decimal, SqlDbType.Decimal, typeof(decimal), true, "DECIMAL", DbTypeLengthRequirements.Precision | DbTypeLengthRequirements.Scale | DbTypeLengthRequirements.IndicatedByParenthesis)
			, new SqlDbTypeTranslation(DbType.Double, SqlDbType.Float, typeof(double), true)
			, new SqlDbTypeTranslation(DbType.Guid, SqlDbType.UniqueIdentifier, typeof(Guid), true)
			, new SqlDbTypeTranslation(DbType.Int16, SqlDbType.SmallInt, typeof(short), true)
			, new SqlDbTypeTranslation(DbType.Int32, SqlDbType.Int, typeof(int), true)
			, new SqlDbTypeTranslation(DbType.Int64, SqlDbType.BigInt, typeof(long), true)
			, default(DbTypeTranslation) 
			, new SqlDbTypeTranslation(DbType.SByte, SqlDbType.TinyInt, typeof(sbyte), true)
			, new SqlDbTypeTranslation(DbType.Single, SqlDbType.Float, typeof(float), true)
			, new SqlDbTypeTranslation(DbType.String, SqlDbType.NVarChar, typeof(string), true, DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "MAX")
			, new SqlDbTypeTranslation(DbType.Time, SqlDbType.Time, typeof(DateTime), false)
			, new SqlDbTypeTranslation(DbType.UInt16, SqlDbType.Int, typeof(ushort), true)
			, new SqlDbTypeTranslation(DbType.UInt32, SqlDbType.BigInt, typeof(uint), true)
			, new SqlDbTypeTranslation(DbType.UInt64, SqlDbType.BigInt, typeof(ulong), true)
			, new SqlDbTypeTranslation(DbType.VarNumeric, SqlDbType.Decimal, typeof(string), false, "NUMERIC", DbTypeLengthRequirements.None)
			, new SqlDbTypeTranslation(DbType.AnsiStringFixedLength, SqlDbType.Char, typeof(string), false, "CHAR", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "100")
			, new SqlDbTypeTranslation(DbType.StringFixedLength, SqlDbType.NChar, typeof(string), false, "NCHAR", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "100")
			, default(DbTypeTranslation)			
			, new SqlDbTypeTranslation(DbType.Xml, SqlDbType.Xml, typeof(string), false, "XML", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis)
			, new SqlDbTypeTranslation(DbType.DateTime2, SqlDbType.DateTime2, typeof(DateTime), false, "DATETIME2", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "7")
			, new SqlDbTypeTranslation(DbType.DateTimeOffset, SqlDbType.DateTimeOffset, typeof(DateTime), false, "DATETIMEOFFSET", DbTypeLengthRequirements.None)
			};
		static readonly ConcurrentDictionary<string, DbTypeTranslation> __runtimeTypeMappings = new ConcurrentDictionary<string, DbTypeTranslation>();

		static SqlDbTypeTranslations()
		{
			__runtimeTypeMappings.TryAdd(typeof(Char).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.AnsiStringFixedLength, SqlDbType.Char, typeof(char), true, "CHAR", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "1"));
			__runtimeTypeMappings.TryAdd(typeof(Type).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.String, SqlDbType.NVarChar, typeof(Type), true, DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "200"));
			__runtimeTypeMappings.TryAdd(typeof(Char[]).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.String, SqlDbType.NChar, typeof(Char[]), true, "NVARCHAR", DbTypeLengthRequirements.Length | DbTypeLengthRequirements.IndicatedByParenthesis, "MAX"));

			__runtimeTypeMappings.TryAdd(typeof(byte?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Byte, SqlDbType.TinyInt, typeof(byte), true));
			__runtimeTypeMappings.TryAdd(typeof(bool?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Boolean, SqlDbType.Bit, typeof(bool), true));
			__runtimeTypeMappings.TryAdd(typeof(DateTime?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.DateTime, SqlDbType.DateTime, typeof(DateTime), true, "DATETIME", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(decimal?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Decimal, SqlDbType.Decimal, typeof(decimal), true, "DECIMAL", DbTypeLengthRequirements.Precision | DbTypeLengthRequirements.Scale | DbTypeLengthRequirements.IndicatedByParenthesis));
			__runtimeTypeMappings.TryAdd(typeof(double?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Double, SqlDbType.Float, typeof(double), true, "FLOAT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(Guid?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Guid, SqlDbType.UniqueIdentifier, typeof(Guid), true, "UNIQUEIDENTIFIER", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(short?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Int16, SqlDbType.SmallInt, typeof(short), true, "SMALLINT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(int?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Int32, SqlDbType.Int, typeof(int), true, "INT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(long?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Int64, SqlDbType.BigInt, typeof(long), true, "BIGINT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(sbyte?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.SByte, SqlDbType.TinyInt, typeof(sbyte), true, "TINYINT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(float?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.Single, SqlDbType.Float, typeof(float), true, "FLOAT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(ushort?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.UInt16, SqlDbType.Int, typeof(ushort), true, "INT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(uint?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.UInt32, SqlDbType.BigInt, typeof(uint), true, "BIGINT", DbTypeLengthRequirements.None));
			__runtimeTypeMappings.TryAdd(typeof(ulong?).AssemblyQualifiedName, new SqlDbTypeTranslation(DbType.UInt64, SqlDbType.BigInt, typeof(ulong), true, "BIGINT", DbTypeLengthRequirements.ApproximationMapping));
			
		}

		public static DbTypeTranslation TranslateDbType(DbType dbType)
		{
			return DbTypeMap[(int)dbType];
		}

		public static DbTypeTranslation TranslateRuntimeType(Type type)
		{
			Contract.Assert(type != null);

			var result = DbTypeMap.Where(t => 
				t != null 
				&& t.RuntimeType == type 
				&& t.IsDefaultForRuntimeType == true).SingleOrDefault();

			if (result == null)
			{
				if (type.IsEnum)
				{
					return TranslateRuntimeType(Enum.GetUnderlyingType(type));
				}
				else 
				{
					var key = type.AssemblyQualifiedName;
					__runtimeTypeMappings.TryGetValue(key, out result);
				}				
			}
			return result;
		}
	}
}
