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
	/// <typeparam name="Id">the model's identity type</typeparam>
	public interface IDataModelBinder<TModel, Id> : IDataModelBinder
	{
		/// <summary>
		/// Gets the model's mapping.
		/// </summary>
		Mapping<TModel> Mapping { get; }

		/// <summary>
		///   Gets a model command for selecting all models of the type TModel.
		/// </summary>
		/// <returns></returns>
		IDataModelCommand<TModel, DbConnection> GetAllCommand();

		/// <summary>
		///   Gets a create command.
		/// </summary>
		/// <returns></returns>
		IDataModelCommand<TModel, TModel, DbConnection> GetCreateCommand();

		/// <summary>
		///   Gets a delete (by ID) command.
		/// </summary>
		/// <returns></returns>
		IDataModelCommand<TModel, Id, DbConnection> GetDeleteCommand();

		/// <summary>
		///   Gets a read (by ID) command.
		/// </summary>
		/// <returns></returns>
		IDataModelCommand<TModel, Id, DbConnection> GetReadCommand();

		/// <summary>
		///   Gets an update command.
		/// </summary>
		/// <returns></returns>
		IDataModelCommand<TModel, TModel, DbConnection> GetUpdateCommand();

		/// <summary>
		///   Makes a delete-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IDataModelCommand<TModel, TMatch, DbConnection> MakeDeleteMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		/// <summary>
		///   Makes a read-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IDataModelCommand<TModel, TMatch, DbConnection> MakeReadMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		/// <summary>
		///   Makes an update-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IDataModelCommand<TModel, TMatch, DbConnection> MakeUpdateMatchCommand<TMatch>(TMatch match)
			where TMatch : class;
	}
}