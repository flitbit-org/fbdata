using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FlitBit.Data
{
	public sealed class SimpleContextCache<TCacheKey, TKey, TValue> : ContextCache<TCacheKey, TKey, TValue>
		where TCacheKey : class
	{
		private readonly Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
		private readonly IDbContext _cx;

		public SimpleContextCache(TCacheKey cacheKey,
			Action<IDbContext, TCacheKey, IEnumerable<KeyValuePair<TKey, TValue>>> callbackOnEnd, IDbContext cx)
			: base(cacheKey, callbackOnEnd)
		{
			Contract.Requires<ArgumentNullException>(cx != null);
			_cx = cx;
		}

		public override TValue Get(TKey key)
		{
			_cx.IncrementCacheAttempts();
			lock (_items)
			{
				TValue res;
				if (_items.TryGetValue(key, out res))
				{
					_cx.IncrementCacheHits();
					return res;
				}
			}
			return default(TValue);
		}

		public override void Put(TKey key, TValue value)
		{
			lock (_items)
			{
				_cx.IncrementCachePuts();
				if (_items.ContainsKey(key))
				{
					_items[key] = value;
				}
				else
				{
					_items.Add(key, value);
				}
			}
		}

		public override TValue Remove(TKey key)
		{
			lock (_items)
			{
				TValue res;
				if (_items.TryGetValue(key, out res))
				{
					_cx.IncrementCacheRemoves();
					_items.Remove(key);
					return res;
				}
			}
			return default(TValue);
		}

		public override void MarkContextEnd(IDbContext context)
		{
			var cb = CallbackOnContextEnd;
			if (cb != null)
			{
				cb(context, CacheKey, _items.Select(kv => kv));
			}
		}
	}
}