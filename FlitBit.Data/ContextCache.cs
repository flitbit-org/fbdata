using System;
using System.Collections.Generic;

namespace FlitBit.Data
{
	/// <summary>
	/// Abstract context cache.
	/// </summary>
	public abstract class ContextCache
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="cackeKey"></param>
		protected ContextCache(object cackeKey)
		{
			UntypedCacheKey = cackeKey;
		}
		/// <summary>
		/// Gets the cache's untyped cache key.
		/// </summary>
		public object UntypedCacheKey { get; private set; }
		public abstract object UntypedGet(object key);
		public abstract void UntypedPut(object key, object value);
		public abstract object UntypedRemove(object key);

		public abstract void MarkContextEnd(IDbContext context);
	}

	public abstract class ContextCache<TCacheKey, TKey, TValue> : ContextCache
		where TCacheKey : class
	{
		protected Action<IDbContext, TCacheKey, IEnumerable<KeyValuePair<TKey, TValue>>> CallbackOnContextEnd { get; private set; }

		protected ContextCache(TCacheKey cacheKey,
			Action<IDbContext, TCacheKey, IEnumerable<KeyValuePair<TKey, TValue>>> callbackOnEnd)
			: base(cacheKey)
		{
			CallbackOnContextEnd = callbackOnEnd;
		}
		public override object UntypedGet(object key)
		{
			return Get((TKey) key);
		}

		public override void UntypedPut(object key, object value)
		{
			Put((TKey)key, (TValue)value);
		}

		public override object UntypedRemove(object key)
		{
			return Remove((TKey)key);
		}

		public TCacheKey CacheKey { get { return (TCacheKey) UntypedCacheKey; } }

		public abstract TValue Get(TKey key);

		public abstract void Put(TKey key, TValue value);

		public abstract TValue Remove(TKey key);
	}
}
