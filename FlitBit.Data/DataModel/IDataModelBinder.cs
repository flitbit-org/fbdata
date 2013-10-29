using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	///   Binds a model to an underlying database structure.
	/// </summary>
	public interface IDataModelBinder
	{
		/// <summary>
		/// Gets the model's mapping (untyped).
		/// </summary>
		IMapping UntypedMapping { get; }

		/// <summary>
		///   Indicates the binder's mapping stretegy.
		/// </summary>
		MappingStrategy Strategy { get; }

		/// <summary>
		/// Adds the model's DDL to a string builder.
		/// </summary>
		/// <param name="batch"></param>
		/// <param name="members"></param>
		void BuildDdlBatch(StringBuilder batch, IList<Type> members);

		/// <summary>
		/// Initializes the binder.
		/// </summary>
		void Initialize();
	}

	/// <summary>
	///   Binds a model to an underlying database structure.
	/// </summary>
	/// <typeparam name="TModel">the model's type.</typeparam>
	/// <typeparam name="TIdentityKey">the model's identity type</typeparam>
	public interface IDataModelBinder<TModel, in TIdentityKey> : IDataModelBinder
	{
		/// <summary>
		/// Gets the model's mapping.
		/// </summary>
		Mapping<TModel> Mapping { get; }
	}

	/// <summary>
	///   Binds a model to an underlying database structure.
	/// </summary>
	/// <typeparam name="TModel">the model's type.</typeparam>
	/// <typeparam name="TIdentityKey">the model's identity type</typeparam>
	/// <typeparam name="TDbConnection">database connection type TDbConnection</typeparam>
	public interface IDataModelBinder<TModel, in TIdentityKey, in TDbConnection> : IDataModelBinder<TModel, TIdentityKey>
		where TDbConnection: DbConnection
	{
		
		/// <summary>
		///   Gets a model command for selecting all models of the type TModel.
		/// </summary>
		/// <returns></returns>
		IDataModelQueryManyCommand<TModel, TDbConnection> GetAllCommand();

		/// <summary>
		///   Gets a create command.
		/// </summary>
		/// <returns></returns>
		IDataModelQuerySingleCommand<TModel, TDbConnection, TModel> GetCreateCommand();

		/// <summary>
		///   Gets a delete (by ID) command.
		/// </summary>
		/// <returns></returns>
		IDataModelNonQueryCommand<TModel, TDbConnection, TIdentityKey> GetDeleteCommand();

		/// <summary>
		///   Gets a read (by ID) command.
		/// </summary>
		/// <returns></returns>
		IDataModelQuerySingleCommand<TModel, TDbConnection, TIdentityKey> GetReadCommand();

		/// <summary>
		///   Gets an update command.
		/// </summary>
		/// <returns></returns>
		IDataModelQuerySingleCommand<TModel, TDbConnection, TModel> GetUpdateCommand();

		/// <summary>
		/// Creates a command builder for the specified criteria.
		/// </summary>
		/// <param name="criteria"></param>
		/// <typeparam name="TCriteria"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TCriteria> MakeQueryCommand<TCriteria>(
			TCriteria criteria);
		
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam> MakeQueryCommand<TParam>();

		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1> MakeQueryCommand<TParam, TParam1>();
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1> MakeQueryCommand<TParam, TParam1, TParam2>();
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3> MakeQueryCommand<TParam, TParam1, TParam2, TParam3>();
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4>();
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>();
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>();
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <typeparam name="TParam7"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>();
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <typeparam name="TParam7"></typeparam>
		/// <typeparam name="TParam8"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>();

		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <typeparam name="TParam7"></typeparam>
		/// <typeparam name="TParam8"></typeparam>
		/// <typeparam name="TParam9"></typeparam>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>();

	}
}