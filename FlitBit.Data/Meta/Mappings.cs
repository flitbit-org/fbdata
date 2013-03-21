#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.Data.Meta
{
	public class Mappings
	{
		public const int CInvalidOrdinal = -1;

		static Lazy<Mappings> __instance = new Lazy<Mappings>(() => { return new Mappings(); },
																													LazyThreadSafetyMode.PublicationOnly);

		static readonly ConcurrentDictionary<object, IMapping> __mappings = new ConcurrentDictionary<object, IMapping>();

		internal Mappings()
		{}

		public string DefaultConnection { get; private set; }
		public string DefaultSchema { get; private set; }

		public Mapping<T> ForType<T>()
		{
			var key = typeof(T).GetKeyForType();
			IMapping m;
			if (!__mappings.TryGetValue(key, out m))
			{
				var mm = new Mapping<T>();
				if (__mappings.TryAdd(key, mm))
				{
					mm.InitFromMetadata()
						.End();
					return mm;
				}
				__mappings.TryGetValue(key, out m);
			}
			return m as Mapping<T>;
		}

		public Mappings UseDefaultConnection(string connection)
		{
			Contract.Requires(connection != null);
			Contract.Requires(connection.Length > 0);
			this.DefaultConnection = connection;
			return this;
		}

		public Mappings UseDefaultSchema(string schema)
		{
			Contract.Requires(schema != null);
			Contract.Requires(schema.Length > 0);
			this.DefaultSchema = schema;
			return this;
		}

		public static Mappings Instance { get { return __instance.Value; } }

		internal static IMapping AccessMappingFor(Type type)
		{
			// Admittedly this is slow, but it is only called at wireup time while generating mappings and IL.
			return (IMapping) typeof(Mappings).GetMethod("ForType")
																				.MakeGenericMethod(type)
																				.Invoke(__instance.Value, null);
		}

		internal static bool ExistsFor(Type type)
		{
			if (Type.GetTypeCode(type) == TypeCode.Object && !type.IsValueType)
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