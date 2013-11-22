using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using FlitBit.Core.Collections;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal static class OneClassOneTableEmitter
	{
		static readonly Lazy<EmittedModule> LazyModule =
			new Lazy<EmittedModule>(() => RuntimeAssemblies.DynamicAssembly.DefineModule("SqlServerMapping", null),
				LazyThreadSafetyMode.ExecutionAndPublication
				);

		static EmittedModule Module
		{
			get { return LazyModule.Value; }
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type CreateCommand<TDataModel, TImpl>(IMapping<TDataModel> mapping, DynamicSql sql)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, "CreateCommand");

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildCreateCommand(module, typeName, mapping, sql);
				return type;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type UpdateCommand<TDataModel, TImpl>(IMapping<TDataModel> mapping, DynamicSql sql)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, "UpdateCommand");

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildUpdateCommand(module, typeName, mapping, sql);
				return type;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type ReadByIdCommand<TDataModel, TImpl, TIdentityKey>(IMapping<TDataModel> mapping, DynamicSql sql)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, "ReadByIdCommand");

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildReadByIdCommand<TIdentityKey>(module, typeName, mapping, sql);
				return type;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeRepositoryType<TDataModel, TImpl, TIdentityKey>(IMapping<TDataModel> mapping)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, "Repository");

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildRepository<TIdentityKey>(module, typeName, mapping);
				return type;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type DeleteCommand<TDataModel, TImpl, TIdentityKey>(IMapping<TDataModel> mapping, DynamicSql deleteStatement)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, "DeleteCommand");

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildDeleteCommand<TIdentityKey>(module, typeName, mapping, deleteStatement);
				return type;
			}
		}

    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
    internal static Type MakeJoinCommand<TDataModel, TImpl, TJoin, TParam>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
      where TImpl : class, IDataModel, TDataModel, new()
    {
      Contract.Requires<ArgumentNullException>(queryKey != null);
      Contract.Requires<ArgumentException>(queryKey.Length > 0);
      Contract.Requires<ArgumentNullException>(cns != null);
      Contract.Ensures(Contract.Result<Type>() != null);

      var targetType = typeof(TDataModel);
      var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

      var module = Module;
      lock (module)
      {
        var type = module.Builder.GetType(typeName, false, false) ??
                  EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TJoin, TParam>));
        return type;
      }
    }

    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
    internal static Type MakeQueryCommand<TDataModel, TImpl, TParam>(IMapping<TDataModel> mapping, string queryKey, DataModelSqlExpression<TDataModel> sql)
      where TImpl : class, IDataModel, TDataModel, new()
    {
      Contract.Requires<ArgumentNullException>(queryKey != null);
      Contract.Requires<ArgumentException>(queryKey.Length > 0);
      Contract.Requires<ArgumentNullException>(sql != null);
      Contract.Ensures(Contract.Result<Type>() != null);

      var targetType = typeof(TDataModel);
      var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

      var module = Module;
      lock (module)
      {
        var type = module.Builder.GetType(typeName, false, false) ??
                  EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, sql, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam>));
        return type;
      }
    }

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
			where TImpl : class, IDataModel, TDataModel, new()
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel, TImpl>.BuildQueryCommand(module, typeName, mapping, cns, typeof(SqlDataModelQueryCommand<TDataModel, TImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>));
				return type;
			}
		}
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type MakeUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(IMapping<TDataModel> mapping, string queryKey, Constraints cns)
		{
			Contract.Requires<ArgumentNullException>(queryKey != null);
			Contract.Requires<ArgumentException>(queryKey.Length > 0);
			Contract.Requires<ArgumentNullException>(cns != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(TDataModel);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, queryKey);

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
									EmitImplementation<TDataModel>.BuildUpdateCommand(module, typeName, mapping, cns, typeof(SqlDataModelUpdateCommand<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>));
				return type;
			}
		}

		private static class EmitImplementation<TDataModel>
		{
			public static Type BuildUpdateCommand(EmittedModule module, string typeName, IMapping<TDataModel> mapping, Constraints cns, Type baseType)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Ensures(Contract.Result<Type>() != null);

				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
					baseType, null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				var ctor = builder.DefineCtor();
				ctor.DefineParameter("update", typeof(DynamicSql));
				ctor.DefineParameter("offsets", typeof(int[]));
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadArg_1();
					il.LoadArg_2();
					il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(DynamicSql), typeof(int[]) }, null));
				});

				ImplementBindQueryCommand(builder, baseType, mapping, cns);

				builder.Compile();
				return builder.Ref.Target;
			}

      public static void ImplementBindQueryCommand(EmittedClass builder, Type baseType, IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
			{
				var method =
					builder.DefineOverrideMethod(baseType.GetMethod("BindCommand", BindingFlags.NonPublic | BindingFlags.Instance));
				var helper = mapping.GetDbProviderHelper();
				var args = new
				{
					self = 0,
					cmd = 1,
					offsets = 2
				};
				const int paramOffset = 3;
				method.ContributeInstructions((m, il) =>
				{
					var parm = il.DeclareLocal(typeof(SqlParameter));
					var flag = il.DeclareLocal(typeof(bool));
				  var i = 0;
					foreach (var p in sql.ValueParameters)
					{
					  var ofs = paramOffset + (i++);
						Action<ILGenerator> loadSource = (stream) => stream.LoadArg(ofs);
            //if (p.Members != null && p.Members.Length > 0)
            //{
            //  // Optimization: Consider evaluating dotted notation to resolve properties to local variable only once when binding several.
            //  foreach (PropertyInfo prop in p.Members)
            //  {
            //    loadSource(il);
            //    var dotted = il.DeclareLocal(prop.GetTypeOfValue());
            //    il.LoadValue(prop.GetGetMethod());
            //    il.StoreLocal(dotted);
            //    // TODO: test for null and if so fallout to bind DBNull
            //    loadSource = stream => stream.LoadLocal(dotted);
            //  }
            //}
            //else
						{
							var emitter = p.Column.Emitter;
							emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, p.Column, p.Text,
								parm,
								gen => gen.LoadArg(args.cmd),
								loadSource,
								gen => { },
								flag
								);
						}
					}
				});
			}

			public static void ImplementBindQueryCommand(EmittedClass builder, Type baseType, IMapping<TDataModel> mapping, Constraints cns)
			{
				var method =
					builder.DefineOverrideMethod(baseType.GetMethod("BindCommand", BindingFlags.NonPublic | BindingFlags.Instance));
				var helper = mapping.GetDbProviderHelper();
				var args = new
				{
					self = 0,
					cmd = 1,
					offsets = 2
				};
				const int paramOffset = 2;
				method.ContributeInstructions((m, il) =>
				{
					var parm = il.DeclareLocal(typeof(SqlParameter));
					var flag = il.DeclareLocal(typeof(bool));
					foreach (var p in cns.Parameters)
					{
						var arg = p.Argument;
						Action<ILGenerator> loadSource = (stream) => stream.LoadArg(paramOffset + arg.Ordinal);
						if (p.Members != null && p.Members.Length > 0)
						{
							// Optimization: Consider evaluating dotted notation to resolve properties to local variable only once when binding several.
							foreach (PropertyInfo prop in p.Members)
							{
								loadSource(il);
								var dotted = il.DeclareLocal(prop.GetTypeOfValue());
								il.LoadValue(prop.GetGetMethod());
								il.StoreLocal(dotted);
								// TODO: test for null and if so fallout to bind DBNull
								loadSource = stream => stream.LoadLocal(dotted);
							}
						}
						else
						{
							var emitter = p.Column.Emitter;
							emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, p.Column, helper.FormatParameterName(arg.Name),
								parm,
								gen => gen.LoadArg(args.cmd),
								loadSource,
								gen => { },
								flag
								);
						}
					}
				});
			}

		}

		static class EmitImplementation<TDataModel, TImpl>
			where TImpl : class, IDataModel, TDataModel, new()
		{
			internal static Type BuildCreateCommand(EmittedModule module, string typeName, IMapping<TDataModel> mapping, DynamicSql sql)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Ensures(Contract.Result<Type>() != null);
				var baseType = typeof(SingleUpdateQueryCommand<TDataModel, TImpl>);
				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
					baseType, null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				var ctor = builder.DefineCtor();
				ctor.DefineParameter("sql", typeof(DynamicSql));
				ctor.DefineParameter("offsets", typeof(int[]));
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadArg_1();
					il.LoadArg_2();
					il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(DynamicSql), typeof(int[]) }, null));
				});

				ImplementCreateBindCommand(builder, baseType, mapping, sql);

				builder.Compile();
				return builder.Ref.Target;
			}

			static void ImplementCreateBindCommand(EmittedClass builder, Type baseType, IMapping<TDataModel> mapping, DynamicSql sql)
			{
				var method =
					builder.DefineOverrideMethod(baseType.GetMethod("BindCommand", BindingFlags.NonPublic | BindingFlags.Instance));
				var helper = mapping.GetDbProviderHelper();
				var args = new
				{
					self = 0,
					cmd = 1,
					sql = 2,
					model = 3,
					dirty = 4,
					offsets = 5
				};
				method.ContributeInstructions((m, il) =>
				{
					var columnList = il.DeclareLocal(typeof(List<string>));
					var valueList = il.DeclareLocal(typeof(List<string>));
					var parm = il.DeclareLocal(typeof(SqlParameter));
					var flag = il.DeclareLocal(typeof(bool));

					il.New<List<string>>();
					il.StoreLocal(columnList);
					il.New<List<string>>();
					il.StoreLocal(valueList);

					foreach (var column in mapping.Columns)
					{
						if (column.IsTimestampOnInsert || column.IsTimestampOnUpdate)
						{
							il.LoadLocal(columnList);
							il.LoadValue(mapping.QuoteObjectName(column.TargetName));
							il.CallVirtual<List<string>>("Add");
							il.LoadLocal(valueList);
							il.LoadValue(sql.CalculatedTimestampVar);
							il.CallVirtual<List<string>>("Add");
						}
						else if (!column.IsCalculated)
						{
							var col = column;
							var emitter = column.Emitter;
							var bindingName = helper.FormatParameterName(column.DbTypeDetails.BindingName);
							var next = il.DefineLabel();
							il.LoadArgAddress(args.dirty);
							il.LoadArg(args.offsets);
							il.LoadValue(column.Ordinal);
							il.Emit(OpCodes.Ldelem_I4);
							il.Call<BitVector>("get_Item");
							il.LoadValue(0);
							il.CompareEqual();
							il.StoreLocal(flag);
							il.LoadLocal(flag);
							il.BranchIfTrue(next);
							il.LoadLocal(columnList);
							il.LoadValue(mapping.QuoteObjectName(column.TargetName));
							il.CallVirtual<List<string>>("Add");
							il.LoadLocal(valueList);
							il.LoadValue(bindingName);
							il.CallVirtual<List<string>>("Add");
							if (column.IsReference)
							{
								var reftype = col.ReferenceTargetMember.GetTypeOfValue();
								emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, column, bindingName,
									parm,
									gen => gen.LoadArg(args.cmd),
									gen => gen.LoadArg(args.model),
									gen =>
									{
										gen.LoadValue(col.Member.Name);
										gen.CallVirtual(typeof(IDataModel).MatchGenericMethod("GetReferentID", 1,
											reftype, typeof(string)).MakeGenericMethod(reftype));
									},
									flag);
							}
							else
							{
								emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, column, bindingName,
									parm,
									gen => gen.LoadArg(args.cmd),
									gen => gen.LoadArg(args.model),
									gen => gen.CallVirtual(((PropertyInfo)col.Member).GetGetMethod()),
									flag);
							}
							il.MarkLabel(next);
						}
					}

					il.LoadArg(args.cmd);
					il.LoadArg(args.sql);
					il.CallVirtual<DynamicSql>("get_Text");
					il.LoadValue(",\r\n\t");
					il.LoadLocal(columnList);
					il.Call<String>("Join", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(IEnumerable<string>));
					il.LoadValue(",\r\n\t");
					il.LoadLocal(valueList);
					il.Call<String>("Join", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(IEnumerable<string>));
					il.Call<String>("Format", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(object), typeof(object));
					il.CallVirtual<DbCommand>("set_CommandText", typeof(string));
				});
			}

			internal static Type BuildUpdateCommand(EmittedModule module, string typeName, IMapping<TDataModel> mapping, DynamicSql sql)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Ensures(Contract.Result<Type>() != null);

				var baseType = typeof(SingleUpdateQueryCommand<TDataModel, TImpl>);
				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
					baseType, null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				var ctor = builder.DefineCtor();
				ctor.DefineParameter("sql", typeof(DynamicSql));
				ctor.DefineParameter("offsets", typeof(int[]));
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadArg_1();
					il.LoadArg_2();
					il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(DynamicSql), typeof(int[]) }, null));
				});

				ImplementUpdateBindCommand(builder, baseType, mapping, sql);

				builder.Compile();
				return builder.Ref.Target;
			}

			static void ImplementUpdateBindCommand(EmittedClass builder, Type baseType, IMapping<TDataModel> mapping, DynamicSql sql)
			{
				var method =
					builder.DefineOverrideMethod(baseType.GetMethod("BindCommand", BindingFlags.NonPublic | BindingFlags.Instance));
				var helper = mapping.GetDbProviderHelper();
				var args = new
				{
					self = 0,
					cmd = 1,
					sql = 2,
					model = 3,
					dirty = 4,
					offsets = 5
				};
				method.ContributeInstructions((m, il) =>
				{
					var columnList = il.DeclareLocal(typeof(List<string>));
					var parm = il.DeclareLocal(typeof(SqlParameter));
					var flag = il.DeclareLocal(typeof(bool));
					var idcol = mapping.Identity.Columns[0].Column;
					var emitter = idcol.Emitter;
					var bindingName = sql.BindIdentityParameter;

					il.New<List<string>>();
					il.StoreLocal(columnList);

					emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, idcol, bindingName,
						parm,
						gen => gen.LoadArg(args.cmd),
						gen => gen.LoadArg(args.model),
						gen => gen.CallVirtual(((PropertyInfo)idcol.Member).GetGetMethod()),
						flag);
					
					foreach (var column in mapping.Columns)
					{
						if (column.IsTimestampOnUpdate)
						{
							il.LoadLocal(columnList);
							il.LoadValue(String.Concat(mapping.QuoteObjectName(column.TargetName), " = ", sql.CalculatedTimestampVar));
							il.CallVirtual<List<string>>("Add");
						}
						else if (!column.IsCalculated)
						{
							var col = column;
							emitter = column.Emitter;
							bindingName = helper.FormatParameterName(column.DbTypeDetails.BindingName);
							var next = il.DefineLabel();
							il.LoadArgAddress(args.dirty);
							il.LoadArg(args.offsets);
							il.LoadValue(column.Ordinal);
							il.Emit(OpCodes.Ldelem_I4);
							il.Call<BitVector>("get_Item");
							il.LoadValue(0);
							il.CompareEqual();
							il.StoreLocal(flag);
							il.LoadLocal(flag);
							il.BranchIfTrue(next);
							il.LoadLocal(columnList);
							il.LoadValue(String.Concat(mapping.QuoteObjectName(column.TargetName), " = ", bindingName));
							il.CallVirtual<List<string>>("Add");
							if (column.IsReference)
							{
								var reftype = col.ReferenceTargetMember.GetTypeOfValue();
								emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, column, bindingName,
									parm,
									gen => gen.LoadArg(args.cmd),
									gen => gen.LoadArg(args.model),
									gen =>
									{
										gen.LoadValue(col.Member.Name);
										gen.CallVirtual(typeof(IDataModel).MatchGenericMethod("GetReferentID", 1,
											reftype, typeof(string)).MakeGenericMethod(reftype));
									},
									flag);
							}
							else
							{
								emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, column, bindingName,
									parm,
									gen => gen.LoadArg(args.cmd),
									gen => gen.LoadArg(args.model),
									gen => gen.CallVirtual(((PropertyInfo) col.Member).GetGetMethod()),
									flag);
							}
							il.MarkLabel(next);
						}
					}

					il.LoadArg(args.cmd);
					il.LoadArg(args.sql);
					il.CallVirtual<DynamicSql>("get_Text");
					il.LoadValue(",\r\n\t");
					il.LoadLocal(columnList);
					il.Call<String>("Join", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(IEnumerable<string>));
					il.Call<String>("Format", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(object));
					il.CallVirtual<DbCommand>("set_CommandText", typeof(string));
				});
			}

			internal static Type BuildDeleteCommand<TIdentityKey>(EmittedModule module, string typeName, IMapping<TDataModel> mapping, DynamicSql sql)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Requires<ArgumentNullException>(sql != null);
				Contract.Ensures(Contract.Result<Type>() != null);

				var baseType = typeof(SqlDataModelNonQueryCommand<TDataModel, TIdentityKey>);
				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
					baseType, null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				var ctor = builder.DefineCtor();
				ctor.DefineParameter("sql", typeof(DynamicSql));
				ctor.DefineParameter("offsets", typeof(int[]));
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadArg_1();
					il.LoadArg_2();
					il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(DynamicSql), typeof(int[]) }, null));
				});

				ImplementByIdBindCommand<TIdentityKey>(builder, baseType, mapping, sql);

				builder.Compile();
				return builder.Ref.Target;
			}

			internal static Type BuildReadByIdCommand<TIdentityKey>(EmittedModule module, string typeName, IMapping<TDataModel> mapping, DynamicSql sql)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Ensures(Contract.Result<Type>() != null);

				var baseType = typeof(SqlDataModelQuerySingleCommand<TDataModel, TImpl, TIdentityKey>);
				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
					baseType, null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				var ctor = builder.DefineCtor();
				ctor.DefineParameter("sql", typeof(DynamicSql));
				ctor.DefineParameter("offsets", typeof(int[]));
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadArg_1();
					il.LoadArg_2();
					il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(DynamicSql), typeof(int[]) }, null));
				});

				ImplementByIdBindCommand<TIdentityKey>(builder, baseType, mapping, sql);

				builder.Compile();
				return builder.Ref.Target;
			}

			public static Type BuildRepository<TIdentityKey>(EmittedModule module, string typeName, IMapping<TDataModel> mapping)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Ensures(Contract.Result<Type>() != null);

				var baseType = (mapping.IsLookupList) 
          ? typeof(LookupListDataModelRepository<TDataModel, TIdentityKey, SqlConnection>) 
          : typeof(DataModelRepository<TDataModel, TIdentityKey, SqlConnection>);
				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
					baseType, null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				var ctor = builder.DefineCtor();
				ctor.DefineParameter("mapping", typeof(IMapping<TDataModel>));
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadArg_1();
					il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IMapping<TDataModel>) }, null));
				});

				ImplementGetIdentity<TIdentityKey>(builder, baseType, mapping);

				builder.Compile();
				return builder.Ref.Target;
			}

			private static void ImplementGetIdentity<TIdentityKey>(EmittedClass builder, Type baseType, IMapping<TDataModel> mapping)
			{
				var method = builder
					.DefineOverrideMethod(baseType.GetMethod("GetIdentity", BindingFlags.Public | BindingFlags.Instance));
				var args = new
				{
					self = 0,
					model = 1
				};
				var column = mapping.Identity.Columns[0].Column;
				Contract.Assert(column.Member is PropertyInfo);	
				method.ContributeInstructions((m, il) =>
				{
					il.LoadArg(args.model);
					il.CallVirtual(((PropertyInfo) column.Member).GetGetMethod());
				});
			}

			static void ImplementByIdBindCommand<TIdentityKey>(EmittedClass builder, Type baseType,
				IMapping<TDataModel> mapping, DynamicSql sql)
			{
				var method =
					builder.DefineOverrideMethod(baseType.GetMethod("BindCommand", BindingFlags.NonPublic | BindingFlags.Instance));
				var helper = mapping.GetDbProviderHelper();
				var args = new
				{
					self = 0,
					cmd = 1,
					parm = 2,
					offsets = 3
				};
				method.ContributeInstructions((m, il) =>
				{
					var parm = il.DeclareLocal(typeof(SqlParameter));
					var flag = il.DeclareLocal(typeof(bool));
					var column = mapping.Identity.Columns[0].Column;
					var emitter = column.Emitter;
					var bindingName = helper.FormatParameterName(sql.BindIdentityParameter);

					emitter.BindParameterOnDbCommand<SqlParameter>(method.Builder, column, bindingName,
						parm,
						gen => gen.LoadArg(args.cmd),
						gen => gen.LoadArg(args.parm),
						gen => { },
						flag);
				});
			}


			internal static Type BuildQueryCommand(EmittedModule module, string typeName, IMapping<TDataModel> mapping, Constraints cns, Type baseType)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Ensures(Contract.Result<Type>() != null);

				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
					baseType, null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				var ctor = builder.DefineCtor();
				ctor.DefineParameter("all", typeof(DynamicSql));
				ctor.DefineParameter("page", typeof(DynamicSql));
				ctor.DefineParameter("offsets", typeof(int[]));
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadArg_1();
					il.LoadArg_2();
					il.LoadArg_3();
					il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(DynamicSql), typeof(DynamicSql), typeof(int[]) }, null));
				});

				EmitImplementation<TDataModel>.ImplementBindQueryCommand(builder, baseType, mapping, cns);

				builder.Compile();
				return builder.Ref.Target;
			}

      internal static Type BuildQueryCommand(EmittedModule module, string typeName, IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql, Type baseType)
      {
        Contract.Requires<ArgumentNullException>(module != null);
        Contract.Requires<ArgumentNullException>(typeName != null);
        Contract.Requires<ArgumentException>(typeName.Length > 0);
        Contract.Requires<InvalidOperationException>(mapping.HasBinder);
        Contract.Ensures(Contract.Result<Type>() != null);

        var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes,
          baseType, null);
        builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

        var ctor = builder.DefineCtor();
        ctor.DefineParameter("all", typeof(DynamicSql));
        ctor.DefineParameter("page", typeof(DynamicSql));
        ctor.DefineParameter("offsets", typeof(int[]));
        ctor.ContributeInstructions((m, il) =>
        {
          il.LoadArg_0();
          il.LoadArg_1();
          il.LoadArg_2();
          il.LoadArg_3();
          il.Call(baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(DynamicSql), typeof(DynamicSql), typeof(int[]) }, null));
        });

        EmitImplementation<TDataModel>.ImplementBindQueryCommand(builder, baseType, mapping, sql);

        builder.Compile();
        return builder.Ref.Target;
      }

		}

	}
}
