using System;
using System.Collections.Generic;
using System.Data.Common;
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
		void BuildDDLBatch(StringBuilder batch, IList<Type> members);
	}

	/// <summary>
	///   Binds a model to an underlying database structure.
	/// </summary>
	/// <typeparam name="TModel">the model's type.</typeparam>
	/// <typeparam name="TIdentityKey">the model's identity type</typeparam>
	public interface IDataModelBinder<TModel, TIdentityKey> : IDataModelBinder
	{
		/// <summary>
		/// Gets the model's mapping.
		/// </summary>
		Mapping<TModel> Mapping { get; }

		/// <summary>
		///   Gets a model command for selecting all models of the type TModel.
		/// </summary>
		/// <returns></returns>
		IDataModelQueryManyCommand<TModel, DbConnection> GetAllCommand();

		/// <summary>
		///   Gets a create command.
		/// </summary>
		/// <returns></returns>
		IDataModelQuerySingleCommand<TModel, DbConnection, TModel> GetCreateCommand();

		/// <summary>
		///   Gets a delete (by ID) command.
		/// </summary>
		/// <returns></returns>
		IDataModelNonQueryCommand<TModel, DbConnection, TIdentityKey> GetDeleteCommand();

		/// <summary>
		///   Gets a read (by ID) command.
		/// </summary>
		/// <returns></returns>
		IDataModelQuerySingleCommand<TModel, DbConnection, TIdentityKey> GetReadCommand();

		/// <summary>
		///   Gets an update command.
		/// </summary>
		/// <returns></returns>
		IDataModelQuerySingleCommand<TModel, DbConnection, TModel> GetUpdateCommand();

		/// <summary>
		///   Makes a delete-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IDataModelNonQueryCommand<TModel, DbConnection, TMatch> MakeDeleteMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		/// <summary>
		///   Makes a read-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IDataModelQueryManyCommand<TModel, DbConnection, TMatch> MakeReadMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		///// <summary>
		/////   Makes an update-match command.
		///// </summary>
		///// <typeparam name="TMatch">the match's type</typeparam>
		///// <param name="match">an match specification</param>
		///// <returns></returns>
		IDataModelNonQueryCommand<TModel, DbConnection, TMatch, TUpdate> MakeUpdateMatchCommand<TMatch, TUpdate>(TMatch match, TUpdate update)
		  where TMatch : class
			where TUpdate : class;
	}
}