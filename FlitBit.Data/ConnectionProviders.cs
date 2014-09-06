using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FlitBit.Core;
using FlitBit.Data.Configuration;

namespace FlitBit.Data
{
    /// <summary>
    /// Default connection providers implementation.
    /// </summary>
    /// <remarks>
    /// This class is thread safe and designed for concurrent use. This 
    /// implementation expects that connection providers will be few (less 
    /// than a 20 or so), and added during application startup. Connections are 
    /// created by the first provider that indicates it can create the 
    /// connection by name. When providers are interrogated, it occurs in 
    /// priority order (lower priority first). If more than one provider
    /// has the same priority, interrogation proceeds in the order the
    /// providers where added.
    /// </remarks>
    public sealed class ConnectionProviders : IConnectionProviders
    {
        /// <summary>
        /// Default priority used when none is assigned.
        /// </summary>
        public static readonly int BasePriority = 100;

        static readonly Object __Sync = new object();
        static ConnectionProviders __instance;

        /// <summary>
        /// Static access to the IConnectionProivders instance.
        /// </summary>
        public static IConnectionProviders Instance
        {
            get
            {
                var factory = FactoryProvider.Factory;
                if (factory.CanConstruct<IConnectionProviders>())
                {
                    return factory.CreateInstance<IConnectionProviders>();
                }
                return Util.LazyInitializeWithLock(ref __instance, __Sync);
            }
        }
        
        readonly DataModelConfigSection _config;
        readonly ConcurrentDictionary<int, Tuple<int, int, IConnectionProvider>> _providers = new ConcurrentDictionary<int, Tuple<int, int, IConnectionProvider>>();
        object _capture = Tuple.Create(0, new IConnectionProvider[0]);
        int _sequence;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public ConnectionProviders(int defaultPriority) : this(defaultPriority, DataModelConfigSection.Instance) { }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public ConnectionProviders() : this(BasePriority, DataModelConfigSection.Instance) { }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="defaultPriority"></param>
        /// <param name="config"></param>
        public ConnectionProviders(int defaultPriority, DataModelConfigSection config)
        {
            Contract.Requires<ArgumentOutOfRangeException>(defaultPriority >= 0);
            Contract.Requires<ArgumentNullException>(config != null);

            _config = config;
            DefaultPriority = defaultPriority;
            Add(new ConfigurationFileConnectionProvider(), defaultPriority);
        }

        /// <summary>
        /// Indicates whether the provider can create a connection for the specified name.
        /// </summary>
        /// <param name="name">The connection's name.</param>
        /// <returns><em>true</em> if the provider can create the specified connection; otherwise <em>false</em></returns>
        public bool CanCreate(string name)
        {
            var map = _config.MapConnectionStrings[name];
            var n = (map != null) ? map.ToName : name;

            return CaptureProviders().Any(provider => provider.CanCreate(n));
        }

        /// <summary>
        /// Gets a connection for the specified name.
        /// </summary>
        /// <param name="name">The connection's name.</param>
        /// <returns>A connection for the specified name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">thrown if the provider cannot provide a connection for the specified name.</exception>
        public IConnection GetConnection(string name)
        {
            var map = _config.MapConnectionStrings[name];
            var n = (map != null) ? map.ToName : name;

            var first = CaptureProviders().FirstOrDefault(it => it.CanCreate(n));
            if (first == null) throw new ArgumentOutOfRangeException("name", String.Concat("No providers can create connections with the specified name: ", n));
            return first.GetConnection(n);
        }

        /// <summary>
        /// Gets a connection for the specified name, of type TDbConnection.
        /// </summary>
        /// <param name="name">the connection's name</param>
        /// <typeparam name="TDbConnection">the connection's type</typeparam>
        /// <returns>A connection for the specified name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">thrown if the provider cannot provide a connection for the specified name.</exception>
        /// <exception cref="InvalidCastException">thrown if the connection's type is not a TDbConnection.</exception>
        public IConnection<TDbConnection> GetConnection<TDbConnection>(string name) where TDbConnection : System.Data.Common.DbConnection
        {
            var map = _config.MapConnectionStrings[name];
            var n = (map != null) ? map.ToName : name;

            var first = CaptureProviders().FirstOrDefault(it => it.CanCreate(n));
            if (first == null) throw new ArgumentOutOfRangeException("name", String.Concat("No providers can create connections with the specified name: ", n));
            return first.GetConnection<TDbConnection>(n);
        }

        /// <summary>
        /// Gets the default priority assigned to providers.
        /// </summary>
        public int DefaultPriority { get; private set; }

        /// <summary>
        /// Adds a provider with the default priority.
        /// </summary>
        /// <param name="provider">the priority</param>
        public void Add(IConnectionProvider provider) { Add(provider, DefaultPriority); }

        /// <summary>
        /// Adds a provider with the specified priority.
        /// </summary>
        /// <param name="provider">the provider</param>
        /// <param name="priority">the priority; must be greater than or equal to 0 (zero).</param>
        public void Add(IConnectionProvider provider, int priority)
        {
            var seq = Interlocked.Increment(ref _sequence);
            var item = Tuple.Create(priority, seq, provider);
            _providers.TryAdd(seq, item);
        }

        /// <summary>
        /// Removes a connection provider.
        /// </summary>
        /// <param name="provider">the provider.</param>
        public void Remove(IConnectionProvider provider)
        {
            var first = _providers.Values.FirstOrDefault(it => it.Item3 == provider);
            if (first != null)
            {
                Tuple<int, int, IConnectionProvider> unused;
                if (_providers.TryRemove(first.Item2, out unused))
                {
                    Interlocked.Increment(ref _sequence);
                }
            }
        }

// ReSharper disable once ReturnTypeCanBeEnumerable.Local
        IConnectionProvider[] CaptureProviders()
        {
            var res = (Tuple<int, IConnectionProvider[]>)Thread.VolatileRead( ref _capture);
            
            while (true)
            {
                var seq = Thread.VolatileRead(ref _sequence);
                if (seq == res.Item1) break;

                var capture = Tuple.Create(seq, _providers.Values
                    .OrderBy(it => it.Item1)
                    .ThenBy(it => it.Item2)
                    .Select(it => it.Item3).ToArray());
                if (ReferenceEquals(Interlocked.CompareExchange(ref _capture, capture, res), res))
                {
                    res = capture;
                    break;
                }
                res = (Tuple<int, IConnectionProvider[]>)Thread.VolatileRead(ref _capture);
            }
            return res.Item2;
        }

        /// <summary>
        /// Utility method; gets the first connection with the specified name
        /// from configured providers.
        /// </summary>
        /// <param name="name">the connection name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">thrown if the provider cannot provide a connection for the specified name.</exception>
        public static DbConnection GetDbConnection(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Ensures(Contract.Result<DbConnection>() != null);

            return Instance.GetConnection(name).UntypedDbConnection;
        }

        /// <summary>
        /// Utility method; gets the first connection with the specified name
        /// from configured providers.
        /// </summary>
        /// <param name="name">the connection name</param>
        /// <typeparam name="TDbConnection"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">thrown if the provider cannot provide a connection for the specified name.</exception>
        public static TDbConnection GetDbConnection<TDbConnection>(string name)
            where TDbConnection : DbConnection
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Ensures(Contract.Result<TDbConnection>() != null);

            return Instance.GetConnection<TDbConnection>(name).DbConnection;
        }
    }
}