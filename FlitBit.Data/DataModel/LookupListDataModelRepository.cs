#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.Cluster;
using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
    public abstract class LookupListDataModelRepository<TDataModel, TIdentityKey, TDbConnection>
        : DataModelRepository<TDataModel, TIdentityKey, TDbConnection>
        where TDbConnection : DbConnection
    {
        static readonly string[] ClusterCacheObservations =
        {
            "created",
            "updated",
            "deleted"
        };

        class CachedItem
        {
            readonly TDataModel _item;
            readonly DateTime _expires;
            readonly bool _hasTtl;

            internal CachedItem(TDataModel value) { _item = value; }

            internal CachedItem(TDataModel value, TimeSpan ttl)
            {
                _item = value;
                _hasTtl = true;
                _expires = DateTime.Now.Add(ttl);
            }

            internal bool TryTake(DateTime time, out TDataModel model)
            {
                if (time <= _expires)
                {
                    model = _item;
                    return true;
                }
                model = default(TDataModel);
                return false;
            }
        }

        readonly ConcurrentDictionary<TIdentityKey, CachedItem> _localCache =
            new ConcurrentDictionary<TIdentityKey, CachedItem>();

        protected LookupListDataModelRepository(IMapping<TDataModel> mapping)
            : base(mapping)
        {
            Contract.Requires<ArgumentNullException>(mapping != null);
            if (mapping.CacheBehavior.HasFlag(ClusterCacheBehaviors.Cluster))
            {
                SubscribeClusterNotification(typeof(TDataModel).FullName, ClusterCacheObservations);
            }
        }

        public override void ClusterNotify(string observation, string identity, IClusteredMemory mem)
        {
            var id = IdFromString(identity);
            if (String.Equals("deleted", observation, StringComparison.Ordinal))
            {
                CachedItem removed;
                _localCache.TryRemove(id, out removed);
            }
            else if (String.Equals("created", observation, StringComparison.Ordinal)
                     || String.Equals("updated", observation, StringComparison.Ordinal))
            {
                var cacheKey = FormatClusteredMemoryKey(identity);
                byte[] buffer;
                if (mem.TryGet(cacheKey, out buffer))
                {
                    var model = BinaryFormat.RestoreBufferView<TDataModel>(buffer);
                    if (model != null)
                    {
                        var updated = new CachedItem(model, Mapping.CacheTimeToLive);
                        _localCache.AddOrUpdate(id, updated, (k, existing) => updated);
                    }
                }
            }
        }

        public override void PromoteCacheItem<T>(string key, T item, bool created)
        {
            var cloneable = item as ICloneable;
            if (cloneable != null)
            {
                var model = (TDataModel)cloneable.Clone();
                var id = this.GetIdentity(model);
                var updated = new CachedItem(model, Mapping.CacheTimeToLive);
                _localCache.AddOrUpdate(id, updated, (k, existing) => updated);
                base.PromoteCacheItem(key, model, created);
            }
        }

        public override void PromoteCacheItem<T>(string key, TimeSpan ttl, T item, bool created)
        {
            var cloneable = item as ICloneable;
            if (cloneable != null)
            {
                var model = (TDataModel)cloneable.Clone();
                var id = this.GetIdentity(model);
                var updated = new CachedItem(model, ttl);
                _localCache.AddOrUpdate(id, updated, (k, existing) => updated);
                base.PromoteCacheItem(key, ttl, item, created);
            }
        }

        public override void PromoteCacheDeletion(string key)
        {
            var chk = key.Split(':');
            var id = IdFromString(chk[1]);
            CachedItem removed;
            _localCache.TryRemove(id, out removed);
            base.PromoteCacheDeletion(key);
        }

        protected override bool PerformTryCacheRead(IDbContext ctx, TIdentityKey key, out TDataModel res)
        {
            CachedItem item;
            return (_localCache.TryGetValue(key, out item) &&
                    item.TryTake(DateTime.Now, out res))
                   || base.PerformTryCacheRead(ctx, key, out res);
        }
    }
}