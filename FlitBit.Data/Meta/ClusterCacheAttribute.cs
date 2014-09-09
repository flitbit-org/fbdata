using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace FlitBit.Data.Meta
{
    public enum ClusterCacheBehaviors
    {
        None = 0,
        Context = 1,
        Cluster = Context | 1 << 1
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ClusterCacheAttribute : Attribute
    {
        public static readonly string DefaultCacheTimeToLiveString = "0.0:05";

        public ClusterCacheAttribute()
            : this(ClusterCacheBehaviors.Context, DefaultCacheTimeToLiveString) { }

        public ClusterCacheAttribute(ClusterCacheBehaviors behaviors)
            : this(behaviors, DefaultCacheTimeToLiveString) { }

        public ClusterCacheAttribute(ClusterCacheBehaviors behaviors, string ttl)
            : this(behaviors, TimeSpan.Parse(ttl)) { }

        public ClusterCacheAttribute(ClusterCacheBehaviors behaviors, TimeSpan ttl)
        {
            this.Behaviors = behaviors;
            this.CacheTimeToLive = ttl;
        }

        public ClusterCacheBehaviors Behaviors { get; private set; }

        /// <summary>
        ///     Indicates the amount of time an item is allowed to live in the
        ///     cache before it must be evicted.
        /// </summary>
        public TimeSpan CacheTimeToLive { get; private set; }

        public string Include { get; private set; }

        internal static ClusterCacheAttribute GetClusterCacheAttribute(Type typ)
        {
            Contract.Requires<ArgumentNullException>(typ != null);

            return typ.IsDefined(typeof(ClusterCacheAttribute), true)
                       ? typ.GetCustomAttribute<ClusterCacheAttribute>(true)
                       : new ClusterCacheAttribute();
        }
    }
}