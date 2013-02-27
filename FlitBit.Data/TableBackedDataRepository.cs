using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public abstract class TableBackedRepository<TModel, Id> : IDataRepository<TModel, Id>
	{
		protected static readonly string CCacheKey = typeof(TModel).AssemblyQualifiedName;
		readonly ConcurrentBag<AbstractCacheHandler> _cacheHandlers = new ConcurrentBag<AbstractCacheHandler>();
		
		abstract class AbstractCacheHandler
		{
			internal abstract void UpdateCacheItem(IDbContext context, TModel item);			
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
			internal override void UpdateCacheItem(IDbContext context, TModel item)
			{
				var cache = context.EnsureCache<TCacheKey, ConcurrentDictionary<TItemKey, TModel>>(_cacheKey);
				var key = _calculateKey(item);
				cache.AddOrUpdate(key, item, (k, v) => item);
			}	
			internal override void RemoveCacheItem(IDbContext context, TModel item)
			{
				var cache = context.EnsureCache<TCacheKey, ConcurrentDictionary<TItemKey, TModel>>(_cacheKey);
				var key = _calculateKey(item);
				TModel unused;
				cache.TryRemove(key, out unused);
			}
		}

		protected void PerformUpdateCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.UpdateCacheItem(context, item);
			}
		}
		protected void PerformRemoveCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.UpdateCacheItem(context, item);
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
		}

		public abstract Id GetIdentity(TModel model);

		public TModel Create(IDbContext context, TModel model)
		{
			var result = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
			using (var exe = helper.DefineExecutableOnConnection(cn, InsertCommand))
			{
				BindInsertCommand(exe.ParameterBinder, model);

				exe.ExecuteReader(res =>
				{
					var reader = res.Result;
					if (reader.Read())
					{
						result = CreateInstance();
						PopulateInstance(result, res.Executable, reader);
						UpdateCacheItem(context, result);
					}
				});
			}
			return result;
		}

		public TModel Read(IDbContext context, Id id)
		{
			return GetCacheItem(context, CCacheKey, id, PerformDirectRead);				
		}

		protected TModel GetCacheItem<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key, Func<IDbContext, TItemKey, TModel> resolver)
			where TCacheKey: class
		{
			if (cacheKey != null && !context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				var cache = context.EnsureCache<TCacheKey, ConcurrentDictionary<TItemKey, TModel>>(cacheKey);

				TModel res;
				if (cache.TryGetValue(key, out res))
				{
					return res;
				}
			}
			return resolver(context, key);
		}
		
		protected void UpdateCacheItem(IDbContext context, TModel item)
		{
			if (!context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				PerformUpdateCacheItem(context, item);
			}
		}		
		protected void RemoveCacheItem(IDbContext context, TModel item)
		{
			if (!context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				PerformRemoveCacheItem(context, item);	
			}
		}												

		TModel PerformDirectRead(IDbContext context, Id id)
		{
			var result = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
			using (var exe = helper.DefineExecutableOnConnection(cn, InsertCommand))
			{
				BindReadCommand(exe.ParameterBinder, id);

				exe.ExecuteReader(res =>
				{
					var reader = res.Result;
					if (reader.Read())
					{
						result = CreateInstance();
						PopulateInstance(result, res.Executable, reader);
						UpdateCacheItem(context, result);
					}
				});
			}
			return result;
		}

		protected virtual TModel ReadBy<TCacheKey, TItemKey>(IDbContext context, string command, Action<IDataParameterBinder> binder, TCacheKey cacheKey, TItemKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			return GetCacheItem(context, cacheKey, key, (ctx, k) =>
			{
				var result = default(TModel);
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, command))
				{
					if (binder != null) binder(exe.ParameterBinder);

					exe.ExecuteReader(res =>
					{
						var reader = res.Result;
						if (reader.Read())
						{
							result = CreateInstance();
							PopulateInstance(result, res.Executable, reader);
							UpdateCacheItem(context, result);
						}
					});
				}
				return result;
			});
		}
		
		public TModel Update(IDbContext context, TModel model)
		{
			var result = default(TModel);
			var cn = context.SharedOrNewConnection(ConnectionName);
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
			string updateCmd = MakeUpdateCommand(model);
			using (var exe = helper.DefineExecutableOnConnection(cn, updateCmd))
			{
				BindUpdateCommand(exe.ParameterBinder, model);

				exe.ExecuteReader(res =>
				{
					var reader = res.Result;
					if (reader.Read())
					{
						result = CreateInstance();
						PopulateInstance(result, res.Executable, reader);
						UpdateCacheItem(context, result);
					}
				});
			}
			return result;
		}

		public bool Delete(IDbContext context, Id id)
		{
			var cn = context.SharedOrNewConnection(ConnectionName);
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
			using (var exe = helper.DefineExecutableOnConnection(cn, DeleteCommand))
			{
				BindDeleteCommand(exe.ParameterBinder, id);

				return (exe.ExecuteNonQuery() > 0);
			}
		}

		public IEnumerable<TModel> All(IDbContext context)
		{
			throw new NotImplementedException();
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
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, InsertCommand))
				{
					BindInsertCommand(exe.ParameterBinder, model);

					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(TModel)));
						else
						{
							var reader = res.Result;
							var created = CreateInstance();
							if (reader.Read())
							{
								PopulateInstance(created, res.Executable, reader);
							}
							Go.Parallel(() => continuation(null, created));
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
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, ReadCommand))
				{
					BindReadCommand(exe.ParameterBinder, id);

					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(TModel)));
						else
						{
							var reader = res.Result;
							var model = CreateInstance();
							if (reader.Read())
							{
								PopulateInstance(model, res.Executable, reader);
								Go.Parallel(() => continuation(null, model));
							}
							else
							{
								Go.Parallel(() => continuation(null, default(TModel)));
							}
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}

		protected virtual void ReadBy(IDbContext context, string command, Action<IDataParameterBinder> binder, Continuation<TModel> continuation)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);
			Contract.Requires<ArgumentNullException>(continuation != null);
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, command))
				{
					if (binder != null) binder(exe.ParameterBinder);

					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(TModel)));
						else
						{
							var reader = res.Result;
							var model = CreateInstance();
							if (reader.Read())
							{
								PopulateInstance(model, res.Executable, reader);
								Go.Parallel(() => continuation(null, model));
							}
							else
							{
								Go.Parallel(() => continuation(null, default(TModel)));
							}
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
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				string updateCmd = MakeUpdateCommand(model);
				using (var exe = helper.DefineExecutableOnConnection(cn, updateCmd))
				{
					BindUpdateCommand(exe.ParameterBinder, model);

					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(TModel)));
						else
						{
							var reader = res.Result;
							var updated = CreateInstance();
							if (reader.Read())
							{
								PopulateInstance(updated, res.Executable, reader);
							}
							Go.Parallel(() => continuation(null, updated));
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
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, DeleteCommand))
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
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, AllCommand))
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
									PopulateInstance(model, exe, reader);
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
		protected abstract void PopulateInstance(TModel model, IDbExecutable exe, IDataRecord reader);
		protected abstract void BindInsertCommand(IDataParameterBinder binder, TModel model);
		protected abstract void BindReadCommand(IDataParameterBinder binder, Id id);
		protected abstract void BindUpdateCommand(IDataParameterBinder binder, TModel model);
		protected abstract void BindDeleteCommand(IDataParameterBinder binder, Id id);
			
	}	
}
