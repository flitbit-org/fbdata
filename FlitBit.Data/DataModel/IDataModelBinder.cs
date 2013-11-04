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
	public interface IDataModelBinder<TModel, TIdentityKey, TDbConnection> : IDataModelBinder<TModel, TIdentityKey>
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
		/// Makes a repository for the data model.
		/// </summary>
		/// <returns></returns>
		IDataModelRepository<TModel, TIdentityKey, TDbConnection> MakeRepository();

		/// <summary>
		/// Creates a command builder for the specified criteria.
		/// </summary>
		/// <param name="queryKey">unique key identifying the query</param>
		/// <param name="criteria"></param>
		/// <typeparam name="TCriteria"></typeparam>
		/// <returns></returns>
		IDataModelQueryCommandBuilder<TModel, TDbConnection, TCriteria> MakeQueryCommand<TCriteria>(string queryKey,
			TCriteria criteria);
		
		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelQueryCommandBuilder<TModel, TDbConnection, TParam> MakeQueryCommand<TParam>(string queryKey);

		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1> MakeQueryCommand<TParam, TParam1>(string queryKey);

		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2> MakeQueryCommand<TParam, TParam1, TParam2>(string queryKey);

		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3> MakeQueryCommand<TParam, TParam1, TParam2, TParam3>(string queryKey);

		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4>(string queryKey);

		/// <summary>
		/// Makes a query command that binds to the specified parameter types.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(string queryKey);

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
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string queryKey);

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
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string queryKey);

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
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string queryKey);

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
		/// <param name="queryKey">unique key identifying the query</param>
		/// <returns></returns>
		IDataModelCommandBuilder<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string queryKey);

	}
}