#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using FlitBit.Data.Cluster;

namespace FlitBit.Data
{
    public partial class DbContext
    {
        int _cacheAttempts;
        int _cacheHits;
        int _cachePuts;
        int _cacheRemoves;
        int _sequence;

        readonly ConcurrentDictionary<string, CachedItem> _cache = new ConcurrentDictionary<string, CachedItem>();

        readonly ConcurrentDictionary<string, CachedDeletion> _cacheDeletions =
            new ConcurrentDictionary<string, CachedDeletion>();

        class CachedDeletion
        {
            readonly ICachePromotionHandler _handler;

            internal CachedDeletion(int sequence, ICachePromotionHandler handler)
            {
                Sequence = sequence;
                _handler = handler;
            }

            internal int Sequence { get; private set; }

            internal void PromoteItem(string key) { _handler.PromoteCacheDeletion(key); }
        }

        abstract class CachedItem
        {
            internal int Sequence { get; set; }

            internal bool Created { get; set; }

            internal ICachePromotionHandler Handler { get; set; }

            internal CachedItem SetCreated(bool created)
            {
                Created = created;
                return this;
            }

            internal abstract bool TryCast<T>(out T item);

            internal abstract void PromoteItem(string key);
        }

        class CachedItem<TItem> : CachedItem
        {
            readonly object _item;
            readonly TimeSpan _ttl;
            readonly bool _hasTtl;

            internal CachedItem(TItem value, bool created, ICachePromotionHandler handler, int sequence)
            {
                _item = value;
                Sequence = sequence;
                Created = created;
                Handler = handler;
            }

            internal CachedItem(TItem value, TimeSpan ttl, bool created, ICachePromotionHandler handler, int sequence)
            {
                _item = value;
                Sequence = sequence;
                Created = created;
                Handler = handler;
                _hasTtl = true;
                _ttl = ttl;
            }

            internal override bool TryCast<T>(out T item)
            {
                if (typeof(T).IsAssignableFrom(typeof(TItem)))
                {
                    item = (T)_item;
                    return true;
                }
                item = default(T);
                return false;
            }

            internal override void PromoteItem(string key)
            {
                if (_hasTtl)
                {
                    Handler.PromoteCacheItem(key, _ttl, (TItem)_item, Created);
                }
                else
                {
                    Handler.PromoteCacheItem(key, (TItem)_item, Created);
                }
            }
        }

        public int CachePuts { get { return Thread.VolatileRead(ref _cachePuts); } }

        public int CacheAttempts { get { return Thread.VolatileRead(ref _cacheAttempts); } }

        public int CacheHits { get { return Thread.VolatileRead(ref _cacheHits); } }

        public int CacheRemoves { get { return Thread.VolatileRead(ref _cacheRemoves); } }

        public bool TryGet<T>(IClusteredMemory sharedCache, string key, out T item)
        {
            Interlocked.Increment(ref _cacheAttempts);
            CachedDeletion deleted;
            var delseq = 0;
            if (_cacheDeletions.TryGetValue(key, out deleted))
            {
                delseq = deleted.Sequence;
            }

            CachedItem cached;
            if (this._cache.TryGetValue(key, out cached)
                && delseq < cached.Sequence
                && cached.TryCast(out item))
            {
                Interlocked.Increment(ref _cacheHits);
                return true;
            }
            if (sharedCache != null)
            {
                byte[] buffer;
                if (delseq == 0
                    && sharedCache.TryGet(key, out buffer))
                {
                    try
                    {
                        item = BinaryFormat.RestoreBufferView<T>(buffer);
                        if (!EqualityComparer<T>.Default.Equals(item, default(T)))
                        {
                            Interlocked.Increment(ref _cacheHits);
                            return true;
                        }
                    }
                    catch (FormatException)
                    {}
                }
            }
            item = default(T);
            return false;
        }

        /// <summary>
        ///     Puts an item in the context cache.
        /// </summary>
        /// <typeparam name="T">the item's type</typeparam>
        /// <param name="key">the item's key</param>
        /// <param name="item">the item</param>
        /// <param name="created">indicates whether the item was create within the context or whether it was pre-existing</param>
        /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
        public void Put<T>(string key, T item, bool created, ICachePromotionHandler promotion)
        {
            if (!EqualityComparer<T>.Default.Equals(item, default(T)))
            {
                var updated = new CachedItem<T>(item, created, promotion, Interlocked.Increment(ref _sequence));
                _cache.AddOrUpdate(key, k => updated, (k, existing) => updated.SetCreated(created || existing.Created));
                Interlocked.Increment(ref _cachePuts);
            }
        }

        /// <summary>
        ///     Puts an item in the context cache.
        /// </summary>
        /// <typeparam name="T">the item's type</typeparam>
        /// <param name="key">the item's key</param>
        /// <param name="item">the item</param>
        /// <param name="created">indicates whether the item was create within the context or whether it was pre-existing</param>
        /// <param name="ttl">the item's time-to-live</param>
        /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
        public void PutWithExpiration<T>(string key, T item, bool created, TimeSpan ttl,
            ICachePromotionHandler promotion)
        {
            if (!EqualityComparer<T>.Default.Equals(item, default(T)))
            {
                var updated = new CachedItem<T>(item, ttl, created, promotion, Interlocked.Increment(ref _sequence));
                _cache.AddOrUpdate(key, k => updated, (k, existing) => updated.SetCreated(existing.Created));
                Interlocked.Increment(ref _cachePuts);
            }
        }

        /// <summary>
        ///     Deletes an item from the context cache.
        /// </summary>
        /// <param name="key">the item's key</param>
        /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
        public void Delete(string key, ICachePromotionHandler promotion)
        {
            var deleteSequence = Interlocked.Increment(ref _sequence);
            var deleted = new CachedDeletion(deleteSequence, promotion);
            _cacheDeletions.AddOrUpdate(key, k => deleted, (k, existing) => deleted);
            Interlocked.Increment(ref _cacheRemoves);
        }

        void PerformCachePromotion(bool hasTrans, TransactionStatus status)
        {
            if (!hasTrans
                || status == TransactionStatus.Committed)
            {
                Parallel.ForEach(_cache, kvp =>
                {
                    DbContextFlowProvider.Push(this);
                    try
                    {
                        CachedDeletion deletion;
                        if (_cacheDeletions.TryRemove(kvp.Key, out deletion))
                        {
                            if (deletion.Sequence < kvp.Value.Sequence)
                            {
                                kvp.Value.PromoteItem(kvp.Key);
                            }
                            else
                            {
                                deletion.PromoteItem(kvp.Key);

                            }
                        }
                        else
                        {
                            kvp.Value.PromoteItem(kvp.Key);
                        }
                    }
                    finally
                    {
                        DbContextFlowProvider.Pop();
                    }
                });
                Parallel.ForEach(_cacheDeletions, kvp => kvp.Value.PromoteItem(kvp.Key));
            }
        }
    }
}