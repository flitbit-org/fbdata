#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using FlitBit.Core;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Meta
{
    /// <summary>
    ///     Utilities for mappings.
    /// </summary>
    public sealed class Mappings
    {
        /// <summary>
        ///     An invalid ordinal.
        /// </summary>
        public const int CInvalidOrdinal = -1;

        static readonly Lazy<Mappings> Singleton = new Lazy<Mappings>(() => new Mappings(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        readonly Dictionary<object, IMapping> _mappings = new Dictionary<object, IMapping>();
        readonly object _sync = new object();

        internal Mappings() { }

        /// <summary>
        ///     Gets the default connection's name.
        /// </summary>
        public string DefaultConnection { get; private set; }

        /// <summary>
        ///     Gets the default schema.
        /// </summary>
        public string DefaultSchema { get; private set; }

        /// <summary>
        ///     Accesses the single mappings instance.
        /// </summary>
        public static Mappings Instance { get { return Singleton.Value; } }

        /// <summary>
        ///     Accesses a mapping for a type of model.
        /// </summary>
        /// <typeparam name="TModel">model's type</typeparam>
        /// <returns></returns>
        public IMapping<TModel> ForType<TModel>()
        {
            var key = typeof(IMapping<TModel>).GetKeyForType();
            Mapping<TModel> res;
            var mustInit = false;

            lock (_sync)
            {
                IMapping existing;
                if (_mappings.TryGetValue(key, out existing))
                {
                    res = (Mapping<TModel>)existing;
                }
                else
                {
                    _mappings.Add(key, res = new Mapping<TModel>());
                    mustInit = true;
                }
            }
            if (mustInit)
            {
                res.InitFromMetadata();
            }
            return res;
        }

        /// <summary>
        ///     Sets the default connection's name.
        /// </summary>
        /// <param name="connection">the name of a connection</param>
        /// <returns>the mappings</returns>
        public Mappings UseDefaultConnection(string connection)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentException>(connection.Length > 0);
            DefaultConnection = connection;
            return this;
        }

        /// <summary>
        ///     Sets the default schema used when one is not specified for a data model.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Mappings UseDefaultSchema(string schema)
        {
            Contract.Requires<ArgumentNullException>(schema != null);
            Contract.Requires<ArgumentException>(schema.Length > 0);
            DefaultSchema = schema;
            return this;
        }

        internal static IMapping AccessMappingFor(Type type)
        {
            // Admittedly this is slow, but it is only called at wireup time while generating mappings and IL.
            return (IMapping)typeof(Mappings).GetMethod("ForType")
                                             .MakeGenericMethod(type)
                                             .Invoke(Singleton.Value, null);
        }

        internal static bool ExistsFor(Type type)
        {
            if (Type.GetTypeCode(type) == TypeCode.Object
                && !type.IsValueType)
            {
                // The type either comes from this assembly or an assembly that references this one.
                var thisAssemblyName = Assembly.GetExecutingAssembly()
                                               .GetName()
                                               .FullName;
                if (type.Assembly.GetName()
                        .FullName == thisAssemblyName
                    || type.Assembly.GetReferencedAssemblies()
                           .Any(n => n.FullName == thisAssemblyName))
                {
                    var m = AccessMappingFor(type);
                    return m != null;
                }
            }
            return false;
        }
    }
}