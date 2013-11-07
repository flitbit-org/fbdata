#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlitBit.Data
{
	public abstract class DbProviderHelper
	{
		readonly ConcurrentDictionary<Type, MappedDbTypeEmitter> _emitters =
			new ConcurrentDictionary<Type, MappedDbTypeEmitter>();

		protected DbProviderHelper()
		{
			Initialize();
		}
		
		protected virtual void Initialize()
		{
			MapRuntimeType<bool>(new MappedBooleanEmitter());
			MapRuntimeType<bool?>(new MappedNullableBooleanEmitter());
			MapRuntimeType<byte>(new MappedByteEmitter());
			MapRuntimeType<byte[]>(new MappedBinaryEmitter());
			MapRuntimeType<char>(new MappedCharEmitter(DbType.String));
			MapRuntimeType<short>(new MappedInt16Emitter());
			MapRuntimeType<string>(new MappedStringEmitter(DbType.String)); 
			MapRuntimeType<int>(new MappedInt32Emitter());
			MapRuntimeType<long>(new MappedInt16Emitter());
			MapRuntimeType<sbyte>(new MappedSByteEmitter());
			MapRuntimeType<UInt16>(new MappedUInt16Emitter());
			MapRuntimeType<UInt32>(new MappedUInt32Emitter());
			MapRuntimeType<UInt64>(new MappedUInt64Emitter());
			MapRuntimeType<decimal>(new MappedDecimalEmitter());
			MapRuntimeType<double>(new MappedDoubleEmitter());
			MapRuntimeType<float>(new MappedSingleEmitter());
			MapRuntimeType<DateTime>(new MappedDateTimeEmitter());
			MapRuntimeType<DateTimeOffset>(new MappedDateTimeOffsetEmitter());
			MapRuntimeType<Type>(new MappedTypeToStringEmitter(DbType.String)); 
		}

		protected void MapRuntimeType<T>(MappedDbTypeEmitter map)
		{
			var key = typeof(T);
			_emitters.AddOrUpdate(key, map, (k, it) => map);
		}

		public DbProviderFactory Factory { get; protected set; }

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText, CommandType cmdType);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText, CommandType cmdType,
			int cmdTimeout);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, IDbExecutable exe);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, IDbExecutable exe);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText,
			CommandType cmdType);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText,
			CommandType cmdType, int cmdTimeout);

		public abstract string FormatParameterName(string rawParameterName);

		public abstract IDataModelBinder<TModel, Id> GetModelBinder<TModel, Id>(IMapping<TModel> mapping);

		public abstract string GetServerName(DbConnection connection);

		public abstract IDataParameterBinder MakeParameterBinder();

		public abstract IDataParameterBinder MakeParameterBinder(DbCommand cmd);
		public abstract DbTypeTranslation TranslateRuntimeType(Type type);

		public virtual IAsyncResult BeginExecuteNonQuery(DbCommand command, AsyncCallback callback, Object stateObject)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Ensures(Contract.Result<IAsyncResult>() != null);

			throw new NotImplementedException();
		}

		public virtual IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, Object stateObject)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Ensures(Contract.Result<IAsyncResult>() != null);

			throw new NotImplementedException();
		}

		public virtual bool CanRetryTransaction(Exception ex)
		{
			return false;
		}

		/// <summary>
		///   Determines if a catalog (database) exists on the connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="catalog"></param>
		public virtual bool CatalogExists(DbConnection connection, string catalog)
		{
			throw new NotImplementedException();
		}

		public virtual int EndExecuteNonQuery(DbCommand command, IAsyncResult ar)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(ar != null);

			throw new NotImplementedException();
		}

		public virtual DbDataReader EndExecuteReader(DbCommand command, IAsyncResult ar)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(ar != null);
			Contract.Ensures(Contract.Result<DbDataReader>() != null);

			throw new NotImplementedException();
		}

		public virtual string QuoteObjectName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);
			Contract.Ensures(Contract.Result<string>() != null);

			return (name[0] != '[') ? String.Concat('[', name, ']') : name;
		}

		public virtual bool SchemaExists(DbConnection connection, string catalog, string schema)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);

			var sql =
				String.Format(
										 "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE CATALOG_NAME = '{0}' AND SCHEMA_NAME = '{1}'",
										catalog, schema);
			return connection.ImmediateExecuteEnumerable(sql)
											.Any();
		}

		public virtual bool SupportsAsynchronousProcessing(DbConnection connection)
		{
			return false;
		}

		public virtual bool SupportsMultipleActiveResultSets(DbConnection connection)
		{
			return false;
		}

		public void CreateSchema(DbConnection connection, string catalog, string schema)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);

			connection.ImmediateExecuteNonQuery(FormatCreateSchemaCommandText(catalog, schema));
		}

		public bool FunctionExists(DbConnection connection, string catalog, string schema, string fun)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(fun != null);
			Contract.Requires<ArgumentException>(fun.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatFunctionExistsQuery(catalog, schema, fun))
				.Any();
		}

		public bool StoredProcedureExists(DbConnection connection, string catalog, string schema, string storedProcedure)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(storedProcedure != null);
			Contract.Requires<ArgumentException>(storedProcedure.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatProcedureExistsQuery(catalog, schema, storedProcedure))
				.Any();
		}

		public bool TableExists(DbConnection connection, string catalog, string schema, string table)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(table != null);
			Contract.Requires<ArgumentException>(table.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatTableExistsQuery(catalog, schema, table))
				.Any();
		}

		public bool ViewExists(DbConnection connection, string catalog, string schema, string view)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(view != null);
			Contract.Requires<ArgumentException>(view.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatViewExistsQuery(catalog, schema, view))
				.Any();
		}

		protected abstract string FormatCreateSchemaCommandText(string catalogName, string schemaName);

		protected virtual string FormatFunctionExistsQuery(string catalog, string schema, string fun)
		{
			return String.Format(@"SELECT ROUTINE_NAME
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_CATALOG = '{0}'
	AND ROUTINE_SCHEMA = '{1}'
	AND ROUTINE_NAME = '{2}'
	AND ROUTINE_TYPE = 'FUNCTION'", catalog, schema, fun);
		}

		protected virtual string FormatProcedureExistsQuery(string catalog, string schema, string storedProcedure)
		{
			return String.Format(@"SELECT ROUTINE_NAME
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_CATALOG = '{0}'
	AND ROUTINE_SCHEMA = '{1}'
	AND ROUTINE_NAME = '{2}'
	AND ROUTINE_TYPE = 'PROCEDURE'", catalog, schema, storedProcedure);
		}

		protected virtual string FormatTableExistsQuery(string catalogName, string schemaName, string tableName)
		{
			return String.Format(@"SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_CATALOG = '{0}'
	AND TABLE_SCHEMA = '{1}'
	AND TABLE_NAME = '{2}'
	AND TABLE_TYPE = 'BASE TABLE'", catalogName, schemaName, tableName);
		}

		protected virtual string FormatViewExistsQuery(string catalogName, string schemaName, string viewName)
		{
			return String.Format(@"SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_CATALOG = '{0}'
	AND TABLE_SCHEMA = '{1}'
	AND TABLE_NAME = '{2}'", catalogName, schemaName, viewName);
		}
		
		internal MappedDbTypeEmitter GetDbTypeEmitter(IMapping mapping, ColumnMapping column)
		{
			var type = column.RuntimeType;
			if (column.IsReference && column.ReferenceTargetMember != null)
			{
				type = column.ReferenceTargetMember.GetTypeOfValue();
			}
			return GetDbTypeEmitter(mapping, type);
		}

		internal MappedDbTypeEmitter GetDbTypeEmitter(IMapping mapping, Type type)
		{
			MappedDbTypeEmitter emitter;
			if (!_emitters.TryGetValue(type, out emitter))
			{
				if (type.IsEnum)
				{
					var etype = Enum.GetUnderlyingType(type);
					Type emitterType;
					switch (Type.GetTypeCode(etype))
					{
						case TypeCode.Int16:
							emitterType = MakeEnumAsInt16Emitter(type);
							break;
						case TypeCode.Int32:
							emitterType = MakeEnumAsInt32Emitter(type);
							break;
						default:
							throw new NotSupportedException(String.Concat("Unable to map enum type ", type.GetReadableSimpleName(), " to DbType due to unsupported underlying type: ", etype.GetReadableSimpleName(), "."));
					}
					return (MappedDbTypeEmitter)Activator.CreateInstance(emitterType, true);
				}
				emitter = GetMissingDbTypeEmitter(mapping, type);
			}
			return emitter;
		}

		protected virtual Type MakeEnumAsInt32Emitter(Type enumType)
		{
			return typeof (MappedEmumAsInt32Emitter<>).MakeGenericType(enumType);
		}

		protected virtual Type MakeEnumAsInt16Emitter(Type enumType)
		{
			return typeof(MappedEmumAsInt16Emitter<>).MakeGenericType(enumType);
		}
		
		protected virtual MappedDbTypeEmitter GetMissingDbTypeEmitter(IMapping mapping, Type type)
		{
			throw new NotImplementedException(String.Concat("There is no mapping for `", type.GetReadableFullName(), "` registered for the underlying DbProvider."));
		}

		public virtual void EmitCreateSchema(StringBuilder batch, string schemaName)
		{
			throw new NotImplementedException();	
		}

		public virtual string TranslateComparison(ExpressionType exprType, ValueReference left, ValueReference right)
		{
			
			switch (exprType)
			{
				case ExpressionType.Equal:
					if (left.Kind == ValueReferenceKind.Null)
					{
						if (right.Kind == ValueReferenceKind.Null)
						{
							return "1 = 1"; // dumb, but writer expects a condition and writing such an expression is likewise.
						}
						return String.Concat(right.Value, " IS NULL");
					}
					return right.Kind == ValueReferenceKind.Null 
						? String.Concat(left.Value, " IS NULL") 
						: String.Concat(left.Value, " = ", right.Value);
				case ExpressionType.GreaterThan:
					return String.Concat(left.Value, " > ", right.Value);
				case ExpressionType.GreaterThanOrEqual:
					return String.Concat(left.Value, " >= ", right.Value);
				case ExpressionType.LessThan:
					return String.Concat(left.Value, " < ", right.Value);
				case ExpressionType.LessThanOrEqual:
					return String.Concat(left.Value, " <= ", right.Value);
				case ExpressionType.NotEqual:
					if (left.Kind == ValueReferenceKind.Null)
					{
						if (right.Kind == ValueReferenceKind.Null)
						{
							return "0 = 1"; // dumb, but writer expects a condition and writing such an expression is likewise.
						}
						return String.Concat(right.Value, " IS NOT NULL");
					}
					return right.Kind == ValueReferenceKind.Null 
						? String.Concat(left.Value, " IS NOT NULL") 
						: String.Concat(left.Value, " <> ", right.Value);
				}
			throw new ArgumentOutOfRangeException("exprType");

		}
	}
}