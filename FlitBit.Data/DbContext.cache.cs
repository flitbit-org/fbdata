#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using FlitBit.Data.Cluster;
using FlitBit.Data.SPI;

namespace FlitBit.Data
{
  public partial class DbContext
  {
    int _cacheAttempts;
    int _cacheHits;
    int _cachePuts;
    int _cacheRemoves;
    int _sequence = 0;

    readonly ConcurrentDictionary<string, CachedItem> _cache = new ConcurrentDictionary<string, CachedItem>();
    readonly ConcurrentDictionary<string, CachedDeletion> _cacheDeletions = new ConcurrentDictionary<string, CachedDeletion>();

    class CachedDeletion
    {
      internal CachedDeletion(int sequence, ICachePromotionHandler handler)
      {
        Sequence = sequence;
        Handler = handler;
      }
      internal int Sequence { get; private set; }

      internal ICachePromotionHandler Handler { get; private set; }

      internal void PromoteItem(string key) { Handler.PromoteCacheDeletion(key); }
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

    /// <summary>
    /// Tries to get an item from the context cache.
    /// </summary>
    /// <param name="mem"></param>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryGet<T>(IClusteredMemory mem, string key, out T item)
    {
      Interlocked.Increment(ref _cacheAttempts);
      CachedDeletion deleted;
      int delseq = 0;
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
      if (mem != null)
      {
        byte[] buffer;
        if (delseq == 0
            && mem.TryGet(key, out buffer))
        {
          try
          {
            item = BinaryFormat.RestoreBufferView<T>(buffer);
            if (item != null)
            {
              Interlocked.Increment(ref _cacheHits);
              return true;
            }
          }
          catch (FormatException)
          {
          }
        }
      }
      item = default(T);
      return false;
    }

    /// <summary>
    /// Puts an item in the context cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <param name="created"></param>
    /// <param name="promotion"></param>
    public void Put<T>(string key, T item, bool created, ICachePromotionHandler promotion)
    {
      if (item != null)
      {
        var updated = new CachedItem<T>(item, created, promotion, Interlocked.Increment(ref _sequence));
        _cache.AddOrUpdate(key, k => updated, (k, existing) => updated.SetCreated(created || existing.Created));
        Interlocked.Increment(ref _cachePuts);
      }
    }

    /// <summary>
    /// Puts an item in the context cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <param name="created"></param>
    /// <param name="ttl"></param>
    /// <param name="promotion"></param>
    public void PutWithExpiration<T>(string key, T item, bool created, TimeSpan ttl, ICachePromotionHandler promotion)
    {
      if (item != null)
      {
        var updated = new CachedItem<T>(item, ttl, created, promotion, Interlocked.Increment(ref _sequence));
        _cache.AddOrUpdate(key, k => updated, (k, existing) => updated.SetCreated(existing.Created));
        Interlocked.Increment(ref _cachePuts);
      }
    }

    /// <summary>
    /// Deletes an item from the context cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="promotion"></param>
    /// <returns></returns>
    public void Delete(string key, ICachePromotionHandler promotion)
    {
      int deleteSequence = Interlocked.Increment(ref _sequence);
      var deleted = new CachedDeletion(deleteSequence, promotion);
      _cacheDeletions.AddOrUpdate(key, k => deleted, (k, existing) => deleted);
      Interlocked.Increment(ref _cacheRemoves);
    }

    private void PerformCachePromotion(bool hasTrans, TransactionStatus status)
    {
      if (!hasTrans || status == TransactionStatus.Committed)
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