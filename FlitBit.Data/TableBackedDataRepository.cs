using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace FlitBit.Data
{										
	public abstract class TableBackedRepository<TModel, Id> : AbstractCachingRepository<TModel, Id>
	{			
		public TableBackedRepository(string connectionName)
			: base(connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);
		}

		protected override TModel PerformCreate(IDbContext context, TModel model)
		{
			var res = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareInsertCommand(context, cn, cmd, model);
				using (var reader = cmd.ExecuteReader())
				{
					context.IncrementQueryCounter(1);
					if (reader.Read())
					{
						res = CreateInstance();
						PopulateInstance(context, res, reader, state);
					}
				}
			}
			return res;
		}

		protected virtual object PrepareInsertCommand(IDbContext context, DbConnection cn, DbCommand cmd, TModel model)
		{
			cmd.CommandText = InsertCommand;
			BindInsertCommand(Helper.MakeParameterBinder(cmd), model);
			return null;
		}	
		
		protected override TModel PerformRead(IDbContext context, Id id)
		{
			var result = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareReadCommand(context, cn, cmd, id);				
				context.IncrementQueryCounter(1);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result = CreateInstance();
						PopulateInstance(context, result, reader, state);
					}
				}
			}
			return result;
		}

		protected virtual object PrepareReadCommand(IDbContext context, DbConnection cn, DbCommand cmd, Id id)
		{
			cmd.CommandText = ReadCommand;
			BindReadCommand(Helper.MakeParameterBinder(cmd), id);
			return null;
		}
				
		protected override TModel PerformDirectReadBy<TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key)
		{
			var result = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareReadByCommand(context, cn, cmd, command, binder, key);
				using (var reader = cmd.ExecuteReader())
				{
					context.IncrementQueryCounter(1);
					if (reader.Read())
					{
						result = CreateInstance();
						PopulateInstance(context, result, reader, state);
					}
				}
			}
			return result;
		}

		protected virtual object PrepareReadByCommand<TItemKey>(IDbContext context, DbConnection cn, DbCommand cmd, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key)
		{
			cmd.CommandText = command;
			if (binder != null)
			{
				binder(key, Helper.MakeParameterBinder(cmd));
			}
			return null;
		}
		
		protected override IEnumerable<TModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key)
		{
			var result = new List<TModel>();
			var cn = context.SharedOrNewConnection(ConnectionName);
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareQueryByCommand(context, cn, cmd, command, binder, key);
				if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
				context.IncrementQueryCounter(1);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						TModel model = CreateInstance();
						PopulateInstance(context, model, reader, state);
						result.Add(model);
					}
				}
			}
			return result;
		}

		protected virtual object PrepareQueryByCommand<TItemKey>(IDbContext context, DbConnection cn, DbCommand cmd, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key)
		{
			cmd.CommandText = command;
			if (binder != null)
			{
				binder(key, Helper.MakeParameterBinder(cmd));
			}
			return null;
		}

		protected override TModel PerformUpdate(IDbContext context, TModel model)
		{
			var res = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareUpdateCommand(context, cn, cmd, model);
				if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
				using (var reader = cmd.ExecuteReader())
				{
					context.IncrementQueryCounter(1);
					if (reader.Read())
					{
						res = CreateInstance();
						PopulateInstance(context, res, reader, state);
					}
				}
			}
			return res;
		}

		protected virtual object PrepareUpdateCommand(IDbContext context, DbConnection cn, DbCommand cmd, TModel model)
		{
			cmd.CommandText = MakeUpdateCommand(model);
			BindUpdateCommand(Helper.MakeParameterBinder(cmd), model);
			return null;
		}

		protected override bool PerformDelete(IDbContext context, Id id)
		{
			var result = false;
			var cn = context.SharedOrNewConnection(ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
			using (var cmd = cn.CreateCommand())
			{
				PrepareDeleteCommand(context, cn, cmd, id);
				result = (cmd.ExecuteNonQuery() > 0);
				context.IncrementQueryCounter(1);
			}
			return result;
		}

		private void PrepareDeleteCommand(IDbContext context, DbConnection cn, DbCommand cmd, Id id)
		{
			cmd.CommandText = DeleteCommand;
			BindDeleteCommand(Helper.MakeParameterBinder(cmd), id);
		}

		protected override IEnumerable<TModel> PerformAll(IDbContext context, QueryBehavior behavior)
		{
			var result = new List<TModel>();
			var cn = context.SharedOrNewConnection(ConnectionName);
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareAllCommand(context, cn, cmd);
				if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
				context.IncrementQueryCounter(1);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						TModel model = CreateInstance();
						PopulateInstance(context, model, reader, state);
						result.Add(model);
					}
				}
			}
			return result;
		}

		protected virtual object PrepareAllCommand(IDbContext context, DbConnection cn, DbCommand cmd)
		{
			cmd.CommandText = AllCommand;
			return null;
		}

		public override IEnumerable<TModel> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match)
		{
			throw new NotImplementedException();
		}

		public override int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)
		{
			throw new NotImplementedException();
		}

		public override int DeleteMatch<TMatch>(IDbContext context, TMatch match)
		{
			throw new NotImplementedException();
		}

		public override IQueryable<TModel> Query()
		{
			throw new NotImplementedException();
		}

		protected string InsertCommand { get; set; }
		protected string ReadCommand { get; set; }
		protected string AllCommand { get; set; }
		protected abstract string MakeUpdateCommand(TModel model);
		protected string DeleteCommand { get; set; }
		protected abstract TModel CreateInstance();
		protected abstract void PopulateInstance(IDbContext context, TModel model, IDataRecord reader, object state);
		protected abstract void BindInsertCommand(IDataParameterBinder binder, TModel model);
		protected abstract void BindReadCommand(IDataParameterBinder binder, Id id);
		protected abstract void BindUpdateCommand(IDataParameterBinder binder, TModel model);
		protected abstract void BindDeleteCommand(IDataParameterBinder binder, Id id);

	}
}
