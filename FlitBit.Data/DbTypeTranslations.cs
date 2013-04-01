#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace FlitBit.Data
{
	internal static class DbTypeTranslations
	{
		static readonly DbTypeTranslation[] DbTypeMap = new DbTypeTranslation[]
		{
			new DbTypeTranslation(DbType.AnsiString, (int) DbType.AnsiString, typeof(string), false, "VARCHAR",
														DbTypeLengthRequirements.Length)
			,
			new DbTypeTranslation(DbType.Binary, (int) DbType.Binary, typeof(byte[]), true, "BINARY",
														DbTypeLengthRequirements.Length)
			,
			new DbTypeTranslation(DbType.Byte, (int) DbType.Byte, typeof(byte), true, "TINYINT", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Boolean, (int) DbType.Boolean, typeof(bool), true, "BIT", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Currency, (int) DbType.Currency, typeof(decimal), false, "MONEY",
														DbTypeLengthRequirements.Length)
			,
			new DbTypeTranslation(DbType.Date, (int) DbType.Date, typeof(DateTime), false, "DATE", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.DateTime, (int) DbType.DateTime, typeof(DateTime), true, "DATETIME",
														DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Decimal, (int) DbType.Decimal, typeof(decimal), true, "DECIMAL",
														DbTypeLengthRequirements.Precision | DbTypeLengthRequirements.Scale)
			,
			new DbTypeTranslation(DbType.Double, (int) DbType.Double, typeof(double), true, "FLOAT",
														DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Guid, (int) DbType.Guid, typeof(Guid), true, "UNIQUEIDENTIFIER",
														DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Int16, (int) DbType.Int16, typeof(short), true, "SMALLINT",
														DbTypeLengthRequirements.None)
			, new DbTypeTranslation(DbType.Int32, (int) DbType.Int32, typeof(int), true, "INT", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Int64, (int) DbType.Int64, typeof(long), true, "BIGINT", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Object, (int) DbType.Object, typeof(long), true, "OBJECT",
														DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.SByte, (int) DbType.SByte, typeof(byte), true, "TINYINT", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.Single, (int) DbType.Single, typeof(float), true, "FLOAT",
														DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.String, (int) DbType.String, typeof(string), true, "NVARCHAR",
														DbTypeLengthRequirements.Length)
			,
			new DbTypeTranslation(DbType.Time, (int) DbType.Time, typeof(DateTime), false, "TIME", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.UInt16, (int) DbType.UInt16, typeof(ushort), true, "INT", DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.UInt32, (int) DbType.UInt32, typeof(uint), true, "BIGINT",
														DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.UInt64, (int) DbType.UInt64, typeof(ulong), true, "BIGINT")
			,
			new DbTypeTranslation(DbType.VarNumeric, (int) DbType.VarNumeric, typeof(string), false, "NUMERIC",
														DbTypeLengthRequirements.None)
			,
			new DbTypeTranslation(DbType.AnsiStringFixedLength, (int) DbType.AnsiStringFixedLength, typeof(string), false,
														"CHAR", DbTypeLengthRequirements.Length)
			,
			new DbTypeTranslation(DbType.StringFixedLength, (int) DbType.StringFixedLength, typeof(string), false, "NCHAR",
														DbTypeLengthRequirements.Length)
			, default(DbTypeTranslation)
			,
			new DbTypeTranslation(DbType.Xml, (int) DbType.Xml, typeof(string), false, "XML",
														DbTypeLengthRequirements.Length)
			,
			new DbTypeTranslation(DbType.DateTime2, (int) DbType.DateTime2, typeof(DateTime), false, "DATETIME2",
														DbTypeLengthRequirements.Length)
			,
			new DbTypeTranslation(DbType.DateTimeOffset, (int) DbType.DateTimeOffset, typeof(DateTime), false, "DATETIMEOFFSET",
														DbTypeLengthRequirements.None)
		};

		static readonly Lazy<ConcurrentDictionary<string, DbTypeTranslation>> __runtimeTypeMappings =
			new Lazy<ConcurrentDictionary<string, DbTypeTranslation>>(LazyThreadSafetyMode.ExecutionAndPublication);

		public static DbTypeTranslation TranslateDbType(DbType dbType)
		{
			return DbTypeMap[(int) dbType];
		}

		public static DbTypeTranslation TranslateRuntimeType(Type type)
		{
			Contract.Assert(type != null);

			var result = DbTypeMap.Where(t =>
																		t != null
																			&& t.RuntimeType == type
																			&& t.IsDefaultForRuntimeType == true)
														.SingleOrDefault();

			if (result == null)
			{
				if (type.IsEnum)
				{
					return TranslateRuntimeType(Enum.GetUnderlyingType(type));
				}
				else if (__runtimeTypeMappings.IsValueCreated)
				{
					var runtimeTypeMappings = __runtimeTypeMappings.Value;
					var key = type.AssemblyQualifiedName;
					runtimeTypeMappings.TryGetValue(key, out result);
				}
			}
			return result;
		}
	}
}