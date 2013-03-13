using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public class DbExecutableDefinition : Disposable, IDbExecutable
	{
		DataParameterBinder _binder;

		public DbExecutableDefinition() { }
		public DbExecutableDefinition(string cmdText) { this.CommandText = cmdText; }

		public DbExecutableDefinition(string cmdText, CommandType cmdType)
		{
			this.CommandText = cmdText;
			this.CommandType = cmdType;
		}

		public DbExecutableDefinition(string cmdText, CommandType cmdType, int cmdTimeout)
		{
			this.CommandText = cmdText;
			this.CommandType = cmdType;
			this.CommandTimeout = cmdTimeout;
		}

		public CommandBehaviors Behaviors { get; internal set; }

		public string CommandText { get; internal set; }

		public CommandType CommandType { get; internal set; }

		public int CommandTimeout { get; internal set; }

		public string ConnectionName { get; internal set; }

		public bool IsExecutableCommand { get; internal set; }

		public IDbContext Context { get; internal set; }

		/// <summary>
		///   Ensures the command includes the given behaviors.
		/// </summary>
		/// <param name="behaviors">behaviors being included</param>
		/// <returns></returns>
		public IDbExecutable IncludeBehaviors(CommandBehaviors behaviors)
		{
			Behaviors |= behaviors;
			return this;
		}

		/// <summary>
		///   Ensures the command DOES NOT include the given behaviors.
		/// </summary>
		/// <param name="behaviors">behaviors being excluded</param>
		public IDbExecutable ExcludeBehaviors(CommandBehaviors behaviors)
		{
			Behaviors &= (~behaviors);
			return this;
		}

		public int ExecuteNonQuery() { throw new NotImplementedException(); }

		public void ExecuteReader(Action<DbResult<DbDataReader>> action) { throw new NotImplementedException(); }

		public IEnumerable<IDataRecord> ExecuteEnumerable() { throw new NotImplementedException(); }

		public T ExecuteScalar<T>() { throw new NotImplementedException(); }

		public void ExecuteNonQuery(Continuation<DbResult<int>> continuation) { throw new NotImplementedException(); }

		public void ExecuteReader(Continuation<DbResult<DbDataReader>> continuation) { throw new NotImplementedException(); }

		public void ExecuteScalar<T>(Continuation<DbResult<T>> continuation) { throw new NotImplementedException(); }

		public IDbExecutable CreateOnConnection(DbConnection connection)
		{
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			return helper.DefineExecutableOnConnection(connection, this);
		}

		public IDbExecutable CreateOnConnection(string connection) { throw new NotImplementedException(); }

		/// <summary>
		///   Gets the command's parameter bindings.
		/// </summary>
		public IDataParameterBinder ParameterBinder
		{
			get { return Util.NonBlockingLazyInitializeVolatile(ref _binder); }
		}

		/// <summary>
		///   Gets the command's parameter bindings.
		/// </summary>
		public IEnumerable<ParameterBinding> Bindings
		{
			get
			{
				return (_binder != null)
					? _binder.Bindings
					: Enumerable.Empty<ParameterBinding>();
			}
		}

		protected override bool PerformDispose(bool disposing) { return true; }
	}
}