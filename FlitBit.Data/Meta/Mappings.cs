﻿#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Meta
{
	/// <summary>
	/// Utilities for mappings.
	/// </summary>
	public sealed class Mappings
	{
		public const int CInvalidOrdinal = -1;

		static readonly Lazy<Mappings> __instance = new Lazy<Mappings>(() => { return new Mappings(); },
																													LazyThreadSafetyMode.PublicationOnly);
		
		internal Mappings()
		{}

		/// <summary>
		/// Gets the default connection's name.
		/// </summary>
		public string DefaultConnection { get; private set; }
		/// <summary>
		/// Gets the default schema.
		/// </summary>
		public string DefaultSchema { get; private set; }

		/// <summary>
		/// Accesses a mapping for a type of model.
		/// </summary>
		/// <typeparam name="TModel">model's type</typeparam>
		/// <returns></returns>
		public Mapping<TModel> ForType<TModel>() { return Mapping<TModel>.Instance; }

		/// <summary>
		/// Sets the default connection's name.
		/// </summary>
		/// <param name="connection">the name of a connection</param>
		/// <returns>the mappings</returns>
		public Mappings UseDefaultConnection(string connection)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);
			this.DefaultConnection = connection;
			return this;
		}

		/// <summary>
		/// Sets the default schema used when one is not specified for a data model.
		/// </summary>
		/// <param name="schema"></param>
		/// <returns></returns>
		public Mappings UseDefaultSchema(string schema)
		{
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires<ArgumentException>(schema.Length > 0);
			this.DefaultSchema = schema;
			return this;
		}

		/// <summary>
		/// Accesses the single mappings instance.
		/// </summary>
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