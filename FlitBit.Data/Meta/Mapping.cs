#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.Data.Meta
{
	public class Mapping
	{
		public const int CInvalidOrdinal = -1;

		readonly static ConcurrentDictionary<object, IMapping> __mappings = new ConcurrentDictionary<object, IMapping>();
		
		static Lazy<Mapping> __instance = new Lazy<Mapping>(() => { return new Mapping(); }, LazyThreadSafetyMode.PublicationOnly);
		internal Mapping()
		{
		}

		public static Mapping Instance
		{
			get { return __instance.Value; }
		}

		public Mapping<T> ForType<T>()
		{
			var key = typeof(T).GetKeyForType();
			IMapping m;
			if (!__mappings.TryGetValue(key, out m))
			{
				var mm = new Mapping<T>();
				if (__mappings.TryAdd(key, mm))
				{
					mm.InitFromMetadata().End();
					return mm;
				}
				__mappings.TryGetValue(key, out m);
			}
			return m as Mapping<T>;
		}
				
		public Mapping UseDefaultSchema(string schema)
		{
			Contract.Requires(schema != null);
			Contract.Requires(schema.Length > 0);
			this.DefaultSchema = schema;
			return this;
		}						

		public Mapping UseDefaultConnection(string connection)
		{
			Contract.Requires(connection != null);
			Contract.Requires(connection.Length > 0);
			this.DefaultConnection = connection;
			return this;
		}

		internal static IMapping AccessMappingFor(Type type)
		{
			// Admittedly this is slow, but it is only called at wireup time while generating IL.
			return (IMapping)typeof(Mapping).GetMethod("ForType").MakeGenericMethod(type).Invoke(__instance.Value, null);
		}

		internal static bool ExistsFor(Type type)
		{
			if (Type.GetTypeCode(type) == TypeCode.Object && !type.IsValueType)
			{
				IMapping m = AccessMappingFor(type);
				return m != null;
			}
			return false;
		}

		public string DefaultSchema { get; private set; }
		public string DefaultConnection { get; private set; }

		public static Mapping<T> BinderFor<T>()
		{
			throw new NotImplementedException();
		}
	}  
}
