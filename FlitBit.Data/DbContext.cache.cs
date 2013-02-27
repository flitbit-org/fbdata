#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Data
{
	public partial class DbContext
	{
		Lazy<ConcurrentDictionary<object, object>> _localCaches = new Lazy<ConcurrentDictionary<object, object>>(LazyThreadSafetyMode.PublicationOnly);

		public C EnsureCache<K, C>(K key, Func<C> factory)
		{
			Contract.Requires<InvalidOperationException>(!Behaviors.HasFlag(DbContextBehaviors.DisableCaching));

			if (_parent != null && _behaviors.HasFlag(DbContextBehaviors.DisableInheritedCache))
			{
				return _parent.EnsureCache(key, factory);
			}
			else
			{
				var caches = _localCaches.Value;
				return (C)caches.GetOrAdd(key, ignored => factory());
			}
		}

		public C EnsureCache<K, C>(K key)
			where C : new()
		{
			Contract.Requires<InvalidOperationException>(!Behaviors.HasFlag(DbContextBehaviors.DisableCaching));

			if (_parent != null && _behaviors.HasFlag(DbContextBehaviors.DisableInheritedCache))
			{
				return _parent.EnsureCache<K, C>(key);
			}
			else
			{
				var caches = _localCaches.Value;
				return (C)caches.GetOrAdd(key, ignored => new C());
			}
		}		
	}
}
