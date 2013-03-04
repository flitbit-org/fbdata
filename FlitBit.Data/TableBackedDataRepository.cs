using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public abstract class TableBackedRepository<TModel, Id> : IDataRepository<TModel, Id>
	{
		protected static readonly string CCacheKey = typeof(TModel).AssemblyQualifiedName;
		readonly ConcurrentBag<AbstractCacheHandler> _cacheHandlers = new ConcurrentBag<AbstractCacheHandler>();
		readonly DbProviderHelper _helper;

		abstract class AbstractCacheHandler
		{
			internal abstract void PerformUpdateCacheItem(IDbContext context, TModel item);
			internal abstract void RemoveCacheItem(IDbContext context, TModel item);
		}
		class CacheHandler<TCacheKey, TItemKey> : AbstractCacheHandler
		{
			private TCacheKey _cacheKey;
			private Func<TModel, TItemKey> _calculateKey;

			public CacheHandler(TCacheKey cacheKey, Func<TModel, TItemKey> calculateKey)
			{
				this._cacheKey = cacheKey;
				this._calculateKey = calculateKey;
			}
			internal override void PerformUpdateCacheItem(IDbContext context, TModel item)
			{
				context.PutCacheItem(_cacheKey, item, _calculateKey(item), (k, v) => item);
			}
			internal override void RemoveCacheItem(IDbContext context, TModel item)
			{
				context.RemoveCacheItem(_cacheKey, item, _calculateKey(item));
			}
		}

		protected void PerformUpdateCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}
		protected void PerformRemoveCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}

		protected void RegisterCacheHandler<TCacheKey, TKey>(TCacheKey cacheKey, Func<TModel, TKey> calculateKey)
		{
			_cacheHandlers.Add(new CacheHandler<TCacheKey, TKey>(cacheKey, calculateKey));
		}

		public TableBackedRepository(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);

			this.ConnectionName = connectionName;
			this.RegisterCacheHandler(CCacheKey, GetIdentity);
			this._helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(connectionName);
		}

		public abstract Id GetIdentity(TModel model);

		public TModel Create(IDbContext context, TModel model)
		{
			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);

			var result = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareInsertCommand(context, cn, cmd, model);
				context.IncrementQueryCounter(1);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result = CreateInstance();
						PopulateInstance(context, result, reader, state);
						if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, result));
					}
				}
			}
			return result;
		}

		protected virtual object PrepareInsertCommand(IDbContext context, DbConnection cn, DbCommand cmd, TModel model)
		{
			cmd.CommandText = InsertCommand;
			BindInsertCommand(_helper.MakeParameterBinder(cmd), model);
			return null;
		}

		public TModel Read(IDbContext context, Id id)
		{
			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformDirectRead(context, id, true);
			}
			else
			{
				return GetCacheItem(context, CCacheKey, id, PerformDirectRead);
			}

		}

		protected TModel GetCacheItem<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key, Func<IDbContext, TItemKey, bool, TModel> resolver)
			where TCacheKey : class
		{
			if (cacheKey != null)
			{
				TModel res;
				if (context.TryGetCacheItem(cacheKey, key, out res))
				{
					return res;
				}
			}
			return resolver(context, key, false);
		}

		protected virtual TModel PerformDirectRead(IDbContext context, Id id, bool disableCaching)
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
						if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, result));
					}
				}
			}
			return result;
		}

		protected virtual object PrepareReadCommand(IDbContext context, DbConnection cn, DbCommand cmd, Id id)
		{
			cmd.CommandText = ReadCommand;
			BindReadCommand(_helper.MakeParameterBinder(cmd), id);
			return null;
		}

		protected TModel ReadBy<TCacheKey, TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TCacheKey cacheKey, TItemKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheItem(context, cacheKey, key, (ctx, k, d) => PerformDirectReadBy(context, command, binder, key, d));
			}
			else
			{
				return PerformDirectReadBy(context, command, binder, key, true);
			}
		}

		protected virtual TModel PerformDirectReadBy<TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key, bool disableCaching)
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
						if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, result));
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
				binder(key, _helper.MakeParameterBinder(cmd));
			}
			return null;
		}

		protected IEnumerable<TModel> QueryBy<TCacheKey, TQueryKey>(IDbContext context, string command, Action<TQueryKey, IDataParameterBinder> binder, TCacheKey cacheKey, TQueryKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheCollection(context, cacheKey, key, (ctx, k, d) => PerformDirectQueryBy(context, command, binder, key, d));
			}
			else
			{
				return PerformDirectQueryBy(context, command, binder, key, true);
			}
		}

		protected IEnumerable<TModel> GetCacheCollection<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key, Func<IDbContext, TItemKey, bool, IEnumerable<TModel>> resolver)
			where TCacheKey : class
		{
			if (cacheKey != null)
			{
				IEnumerable<TModel> res;
				if (context.TryGetCacheItem(cacheKey, key, out res))
				{

					return res;
				}
			}
			return resolver(context, key, false);
		}

		protected virtual IEnumerable<TModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key, bool disableCaching)
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
						if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, model));
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
				binder(key, _helper.MakeParameterBinder(cmd));
			}
			return null;
		}

		public TModel Update(IDbContext context, TModel model)
		{
			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);

			var result = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			using (var cmd = cn.CreateCommand())
			{
				object state = PrepareUpdateCommand(context, cn, cmd, model);
				if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
				context.IncrementQueryCounter(1);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result = CreateInstance();
						PopulateInstance(context, result, reader, state);
						if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, result));
					}
				}
			}
			return result;
		}

		protected virtual object PrepareUpdateCommand(IDbContext context, DbConnection cn, DbCommand cmd, TModel model)
		{
			cmd.CommandText = MakeUpdateCommand(model);
			BindUpdateCommand(_helper.MakeParameterBinder(cmd), model);
			return null;
		}

		public bool Delete(IDbContext context, Id id)
		{
			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);

			var result = false;
			var cn = context.SharedOrNewConnection(ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
			context.IncrementQueryCounter(1);
			using (var cmd = cn.CreateCommand())
			{
				PrepareDeleteCommand(context, cn, cmd, id);
				result = (cmd.ExecuteNonQuery() > 0);
			}
			return result;
		}

		private void PrepareDeleteCommand(IDbContext context, DbConnection cn, DbCommand cmd, Id id)
		{
			cmd.CommandText = DeleteCommand;
			BindDeleteCommand(_helper.MakeParameterBinder(cmd), id);
		}

		public IEnumerable<TModel> All(IDbContext context)
		{
			var result = new List<TModel>();
			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
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
						if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, model));
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

		public IEnumerable<TModel> ReadMatch<TMatch>(IDbContext context, TMatch match)
		{
			throw new NotImplementedException();
		}

		public int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)
		{
			throw new NotImplementedException();
		}

		public int DeleteMatch<TMatch>(IDbContext context, TMatch match)
		{
			throw new NotImplementedException();
		}

		public void UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update, Continuation<int> continuation)
		{
			throw new NotImplementedException();
		}

		public void DeleteMatch<TMatch>(IDbContext context, TMatch match, Continuation<int> continuation)
		{
			throw new NotImplementedException();
		}

		public void Create(IDbContext context, TModel model, Continuation<TModel> continuation)
		{
			try
			{
				bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);

				var cn = context.NewConnection(ConnectionName);
				var _helper = this._helper;
				if (_helper.SupportsAsynchronousProcessing(cn))
				{
					var cmd = cn.CreateCommand();
					cmd.Disposed += (s, e) => { Util.VolatileWrite(ref cmd, null); };

					object state = PrepareInsertCommand(context, cn, cmd, model);
					if (!cn.State.HasFlag(ConnectionState.Open)) cn.Open();
					context.IncrementQueryCounter(1);
					var ar = _helper.BeginExecuteReader(cmd,
						res =>
						{
							try
							{
								if (!res.CompletedSynchronously)
								{
									var mm = default(TModel);
									using (var reader = _helper.EndExecuteReader(cmd, res))
									{
										if (reader.Read())
										{
											mm = CreateInstance();
											PopulateInstance(context, mm, reader, state);
											if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, mm));
										}
									}
									continuation(null, mm);
								}
							}
							catch (Exception e)
							{
								continuation(e, default(TModel));
							}
							finally
							{
								var c = Util.VolatileRead(ref cmd);
								if (c != null) c.Dispose();
							}
						}, null);
					if (ar.CompletedSynchronously)
					{
						try
						{
							if (!ar.CompletedSynchronously)
							{
								var mm = default(TModel);
								using (var reader = _helper.EndExecuteReader(cmd, ar))
								{
									if (reader.Read())
									{
										mm = CreateInstance();
										PopulateInstance(context, mm, reader, state);
										if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, mm));
									}
								}
								continuation(null, mm);
							}
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
						finally
						{
							var c = Util.VolatileRead(ref cmd);
							if (c != null) c.Dispose();
						}
					}
				}
				else
				{
					Go.Parallel(() =>
					{
						try
						{
							continuation(null, Create(context, model));
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}

		public void Read(IDbContext context, Id id, Continuation<TModel> continuation)
		{
			try
			{
				bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);

				var cleanup = new CleanupScope(true);
				var cn = cleanup.Add(context.NewConnection(ConnectionName));
				cn.Open();
				var _helper = this._helper;
				if (_helper.SupportsAsynchronousProcessing(cn))
				{
					var cmd = cleanup.Add(cn.CreateCommand());

					object state = PrepareReadCommand(context, cn, cmd, id);
					context.IncrementQueryCounter(1);
					var ar = _helper.BeginExecuteReader(cmd,
						res =>
						{
							try
							{
								if (!res.CompletedSynchronously)
								{
									var model = default(TModel);
									using (var reader = _helper.EndExecuteReader(cmd, res))
									{
										if (reader.Read())
										{
											model = CreateInstance();
											PopulateInstance(context, model, reader, state);
											if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, model));
										}
									}
									continuation(null, model);
								}
							}
							catch (Exception e)
							{
								continuation(e, default(TModel));
							}
							finally
							{
								cleanup.Dispose();
							}
						}, null);
					if (ar.CompletedSynchronously)
					{
						try
						{
							if (!ar.CompletedSynchronously)
							{
								var model = default(TModel);
								using (var reader = _helper.EndExecuteReader(cmd, ar))
								{
									if (reader.Read())
									{
										model = CreateInstance();
										PopulateInstance(context, model, reader, state);
										if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, model));
									}
								}
								continuation(null, model);
							}
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
						finally
						{
							cleanup.Dispose();
						}
					}
				}
				else
				{
					Go.Parallel(() =>
					{
						try
						{
							continuation(null, Read(context, id));
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}

		protected virtual void ReadBy<TCacheKey, TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TCacheKey cacheKey, TItemKey itemKey, Continuation<TModel> continuation)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);
			Contract.Requires<ArgumentNullException>(continuation != null);
			try
			{
				bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);

				var cleanup = new CleanupScope(true);
				var cn = cleanup.Add(context.NewConnection(ConnectionName));
				cn.Open();
				var _helper = this._helper;
				if (_helper.SupportsAsynchronousProcessing(cn))
				{
					var cmd = cleanup.Add(cn.CreateCommand());

					object state = PrepareReadByCommand(context, cn, cmd, command, binder, itemKey);
					context.IncrementQueryCounter(1);
					var ar = _helper.BeginExecuteReader(cmd,
						res =>
						{
							try
							{
								if (!res.CompletedSynchronously)
								{
									var model = default(TModel);
									using (var reader = _helper.EndExecuteReader(cmd, res))
									{
										if (reader.Read())
										{
											model = CreateInstance();
											PopulateInstance(context, model, reader, state);
											if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, model));
										}
									}
									continuation(null, model);
								}
							}
							catch (Exception e)
							{
								continuation(e, default(TModel));
							}
							finally
							{
								cleanup.Dispose();
							}
						}, null);
					if (ar.CompletedSynchronously)
					{
						try
						{
							if (!ar.CompletedSynchronously)
							{
								var model = default(TModel);
								using (var reader = _helper.EndExecuteReader(cmd, ar))
								{
									if (reader.Read())
									{
										model = CreateInstance();
										PopulateInstance(context, model, reader, state);
										if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, model));
									}
								}
								continuation(null, model);
							}
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
						finally
						{
							cleanup.Dispose();
						}
					}
				}
				else
				{
					Go.Parallel(() =>
					{
						try
						{
							continuation(null, ReadBy(context, command, binder, cacheKey, itemKey));
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}

		public void Update(IDbContext context, TModel model, Continuation<TModel> continuation)
		{
			try
			{
				bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);

				var cleanup = new CleanupScope(true);
				var cn = cleanup.Add(context.NewConnection(ConnectionName));
				cn.Open();
				var _helper = this._helper;
				if (_helper.SupportsAsynchronousProcessing(cn))
				{
					var cmd = cleanup.Add(cn.CreateCommand());

					object state = PrepareUpdateCommand(context, cn, cmd, model);
					context.IncrementQueryCounter(1);
					var ar = _helper.BeginExecuteReader(cmd,
						res =>
						{
							try
							{
								if (!res.CompletedSynchronously)
								{
									var mm = default(TModel);
									using (var reader = _helper.EndExecuteReader(cmd, res))
									{
										if (reader.Read())
										{
											mm = CreateInstance();
											PopulateInstance(context, mm, reader, state);
											if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, mm));
										}
									}
									continuation(null, mm);
								}
							}
							catch (Exception e)
							{
								continuation(e, default(TModel));
							}
							finally
							{
								cleanup.Dispose();
							}
						}, null);
					if (ar.CompletedSynchronously)
					{
						try
						{
							if (!ar.CompletedSynchronously)
							{
								var mm = default(TModel);
								using (var reader = _helper.EndExecuteReader(cmd, ar))
								{
									if (reader.Read())
									{
										mm = CreateInstance();
										PopulateInstance(context, mm, reader, state);
										if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, mm));
									}
								}
								continuation(null, mm);
							}
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
						finally
						{
							cleanup.Dispose();
						}
					}
				}
				else
				{
					Go.Parallel(() =>
					{
						try
						{
							continuation(null, Update(context, model));
						}
						catch (Exception e)
						{
							continuation(e, default(TModel));
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}

		public void Delete(IDbContext context, Id id, Continuation<bool> continuation)
		{
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName).EnsureConnectionIsOpen();
				var _helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = _helper.DefineExecutableOnConnection(cn, DeleteCommand))
				{
					BindDeleteCommand(exe.ParameterBinder, id);

					exe.ExecuteNonQuery((e, res) =>
					{
						if (e != null) Go.Parallel(() => { continuation(e, false); });
						else Go.Parallel(() => continuation(null, (res.Result > 0)));
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, false);
			}
		}

		public void All(IDbContext context, Continuation<IEnumerable<TModel>> continuation)
		{
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName).EnsureConnectionIsOpen();
				using (var exe = _helper.DefineExecutableOnConnection(cn, AllCommand))
				{
					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(IEnumerable<TModel>)));
						else
						{
							List<TModel> result = new List<TModel>();
							var reader = res.Result;
							try
							{
								while (reader.Read())
								{
									var model = CreateInstance();
									PopulateInstance(context, model, reader, null);
									result.Add(model);
								}
								continuation(null, result);
							}
							catch (Exception ee)
							{
								continuation(ee, default(IEnumerable<TModel>));
							}
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(IEnumerable<TModel>));
			}
		}

		public virtual void ReadMatch<TMatch>(IDbContext context, TMatch match, Continuation<IEnumerable<TModel>> continuation)
		{
			throw new NotImplementedException();
		}

		public virtual IQueryable<TModel> Query()
		{
			throw new NotImplementedException();
		}

		protected string ConnectionName { get; private set; }

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

		protected DbProviderHelper Helper { get { return _helper; } }
	}
}
