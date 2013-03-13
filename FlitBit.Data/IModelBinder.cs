using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using FlitBit.Data.Meta;

namespace FlitBit.Data
{
	/// <summary>
	///   Binds a model to an underlying database structure.
	/// </summary>
	/// <typeparam name="TModel">the model's type.</typeparam>
	/// <typeparam name="Id">the model's identity type</typeparam>
	public interface IModelBinder
	{
		IMapping Mapping { get; }

		/// <summary>
		///   Indicates the binder's mapping stretegy.
		/// </summary>
		MappingStrategy Strategy { get; }

		void BuildDdlBatch(StringBuilder batch, IList<Type> members);
	}

	/// <summary>
	///   Binds a model to an underlying database structure.
	/// </summary>
	/// <typeparam name="TModel">the model's type.</typeparam>
	/// <typeparam name="Id">the model's identity type</typeparam>
	public interface IModelBinder<TModel, Id> : IModelBinder
	{
		/// <summary>
		///   Gets an all command.
		/// </summary>
		/// <returns></returns>
		IModelCommand<TModel, DbConnection> GetAllCommand();

		/// <summary>
		///   Gets a create command.
		/// </summary>
		/// <returns></returns>
		IModelCommand<TModel, TModel, DbConnection> GetCreateCommand();

		/// <summary>
		///   Gets a delete (by ID) command.
		/// </summary>
		/// <returns></returns>
		IModelCommand<TModel, Id, DbConnection> GetDeleteCommand();

		/// <summary>
		///   Gets a read (by ID) command.
		/// </summary>
		/// <returns></returns>
		IModelCommand<TModel, Id, DbConnection> GetReadCommand();

		/// <summary>
		///   Gets an update command.
		/// </summary>
		/// <returns></returns>
		IModelCommand<TModel, TModel, DbConnection> GetUpdateCommand();

		/// <summary>
		///   Makes a delete-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IModelCommand<TModel, TMatch, DbConnection> MakeDeleteMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		/// <summary>
		///   Makes a read-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IModelCommand<TModel, TMatch, DbConnection> MakeReadMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		/// <summary>
		///   Makes an update-match command.
		/// </summary>
		/// <typeparam name="TMatch">the match's type</typeparam>
		/// <param name="match">an match specification</param>
		/// <returns></returns>
		IModelCommand<TModel, TMatch, DbConnection> MakeUpdateMatchCommand<TMatch>(TMatch match)
			where TMatch : class;
	}
}