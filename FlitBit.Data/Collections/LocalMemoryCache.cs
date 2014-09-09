using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FlitBit.Data.Collections
{
    /// <summary>
    ///     A strongly typed hashtable that supports alternate keys over the member objects.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class LocalMemoryCache<TData, TKey>
    {
        int _sequence;

        internal class CachedItem
        {
            readonly TData _item;
            readonly DateTime _expires;
            readonly int _sequence;
            readonly bool _hasTtl;

            internal CachedItem(int seq, TData value)
            {
                _sequence = seq;
                _item = value;
            }

            internal CachedItem(int seq, TData value, TimeSpan ttl)
            {
                _sequence = seq;
                _item = value;
                _hasTtl = true;
                _expires = DateTime.Now.Add(ttl);
            }

            internal int Sequence { get { return _sequence; } }

            internal TData Item { get { return _item; } }

            internal bool TryTake(DateTime time, out TData model)
            {
                if (!_hasTtl
                    || time <= _expires)
                {
                    model = _item;
                    return true;
                }
                model = default(TData);
                return false;
            }

            internal bool TryTakeMatchSequence(int sequence, DateTime time, out TData model)
            {
                if (_sequence == sequence)
                {
                    return TryTake(time, out model);
                }
                model = default(TData);
                return false;
            }
        }

        readonly ConcurrentDictionary<TKey, CachedItem> _items = new ConcurrentDictionary<TKey, CachedItem>();
        readonly ConcurrentDictionary<string, Index> _alternates = new ConcurrentDictionary<string, Index>();

        abstract class Index
        {
            internal abstract void Add(TKey key, TData data, int sequence);
            internal abstract void Delete(TKey key, TData data, int sequence);

            internal abstract bool TryGet<TAlternateKey>(TAlternateKey key, ConcurrentDictionary<TKey, CachedItem> items,
                out TData data);
        }

        class Index<TAlternateKey> : Index
        {
            readonly AlternateKey<TAlternateKey> _keys;
            readonly Func<TData, TAlternateKey> _getKey;

            internal Index(Comparison<TAlternateKey> comparison, Func<TData, TAlternateKey> getKey)
            {
                _keys = new AlternateKey<TAlternateKey>(comparison);
                _getKey = getKey;
            }

            internal override void Add(TKey key, TData data, int sequence)
            {
                var ak = _getKey(data);
                _keys.Put(ak, key, sequence);
            }

            internal override void Delete(TKey key, TData data, int sequence)
            {
                var ak = _getKey(data);
                _keys.Delete(ak, key, sequence);
            }

            internal override bool TryGet<TAlternate>(
                TAlternate key,
                ConcurrentDictionary<TKey, CachedItem> items,
                out TData data)
            {
                if (typeof(TAlternateKey).IsAssignableFrom(typeof(TAlternate)))
                {
                    var ak = (TAlternateKey)(object)key;
                    return _keys.TryGet(ak, items, out data);
                }
                data = default(TData);
                return false;
            }
        }

        public bool DefineAlternateKey<TAlternateKey>(string keyName, Comparison<TAlternateKey> comparison,
            Func<TData, TAlternateKey> getKeyFromItem)
        {
            var idx = new Index<TAlternateKey>(comparison, getKeyFromItem);
            return _alternates.TryAdd(keyName, idx);
        }

        public bool UndefineAlternateKey(string keyName)
        {
            Index removed;
            return _alternates.TryRemove(keyName, out removed);
        }

        public void Put(TKey key, TData item)
        {
            var inserted = new CachedItem(Interlocked.Increment(ref _sequence), item);
            this._items.AddOrUpdate(key, inserted, (k, existing) => inserted);
            AddAlternateKeys(key, inserted);
        }

        void AddAlternateKeys(TKey key, CachedItem item)
        {
            var keys = _alternates.Keys;
            foreach (var k in keys)
            {
                Index idx;
                if (_alternates.TryGetValue(k, out idx))
                {
                    idx.Add(key, item.Item, item.Sequence);
                }
            }
        }

        void RemoveAlternateKeys(TKey key, CachedItem item)
        {
            var keys = _alternates.Keys;
            foreach (var k in keys)
            {
                Index idx;
                if (_alternates.TryGetValue(k, out idx))
                {
                    idx.Delete(key, item.Item, item.Sequence);
                }
            }
        }

        public void Put(TKey key, TData item, TimeSpan ttl)
        {
            var inserted = new CachedItem(Interlocked.Increment(ref _sequence), item, ttl);
            this._items.AddOrUpdate(key, inserted, (k, existing) => inserted);
            AddAlternateKeys(key, inserted);
        }

        public void Delete(TKey key)
        {
            CachedItem removed;
            this._items.TryRemove(key, out removed);
            RemoveAlternateKeys(key, removed);
        }

        protected bool TryGet(TKey key, out TData data)
        {
            CachedItem item;
            if (_items.TryGetValue(key, out item)
                &&
                item.TryTake(DateTime.Now, out data))
            {
                return true;
            }
            data = default(TData);
            return false;
        }

        protected bool TryGet<TAlternateKey>(string keyName, TAlternateKey key, out TData data)
        {
            Index idx;
            if (_alternates.TryGetValue(keyName, out idx)
                &&
                idx.TryGet(key, _items, out data))
            {
                return true;
            }
            data = default(TData);
            return false;
        }

        internal class AlternateKey<TAlternateKey>
        {
            readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            readonly LeftLeaningRedBlackTree<TAlternateKey, Tuple<int, TKey>> _tree;

            public AlternateKey(Comparison<TAlternateKey> comparison)
            {
                _tree = new LeftLeaningRedBlackTree<TAlternateKey, Tuple<int, TKey>>(comparison);
            }

            public void Put(TAlternateKey alternateKey, TKey key, int sequence)
            {
                _lock.EnterWriteLock();
                try
                {
                    _tree.Remove(alternateKey);
                    _tree.Add(alternateKey, Tuple.Create(sequence, key));
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            public void Delete(TAlternateKey alternateKey, TKey key, int sequence)
            {
                _lock.EnterWriteLock();
                try
                {
                    _tree.Remove(alternateKey, Tuple.Create(sequence, key));
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            public bool TryGet(TAlternateKey alternateKey, ConcurrentDictionary<TKey, CachedItem> items, out TData data)
            {
                Tuple<int, TKey> it;
                _lock.EnterReadLock();
                try
                {
                    _tree.TryGetValueForKey(alternateKey, out it);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
                if (it != null)
                {
                    CachedItem item;
                    if (items.TryGetValue(it.Item2, out item)
                        && item.TryTakeMatchSequence(it.Item1, DateTime.Now, out data))
                    {
                        return true;
                    }
                }
                data = default(TData);
                return false;
            }
        }
    }
}