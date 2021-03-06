﻿#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	public static class DbExtensions
	{
		static ConcurrentDictionary<string, ProviderRecord> __providers = new ConcurrentDictionary<string, ProviderRecord>();

		/// <summary>
		///   Creates and opens a new DbConnection initialized with the specified connection name.
		///   The connection name must be present in the &lt;connectionStrings&gt; section of the
		///   application's config file
		/// </summary>
		/// <param name="name">A connection string name declared in the application's config file.</param>
		/// <returns>An open connection to the database specified in the connection string.</returns>
		/// <exception cref="System.ContractException">thrown if the connection name is not specified.</exception>
		/// <exception cref="System.Configuration.ConfigurationErrorsException">thrown if the connection name is not found in the application's config file.</exception>
		public static DbConnection CreateAndOpenConnection(string name)
		{
			var result = CreateConnection(name);
			try
			{
				result.Open();
			}
			catch
			{
				result.Dispose();
				throw;
			}
			return result;
		}

		/// <summary>
		///   Creates a new DbConnection initialized with the specified connection name. The connection name
		///   must be present in the &lt;connectionStrings&gt; section of the application's config file
		/// </summary>
		/// <param name="name">A connection string name declared in the application's config file.</param>
		/// <returns>The DbConnection initialized with the specified connection string.</returns>
		/// <exception cref="System.ContractException">thrown if the connection name is not specified.</exception>
		/// <exception cref="System.Configuration.ConfigurationErrorsException">thrown if the connection name is not found in the application's config file.</exception>
		public static DbConnection CreateConnection(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);

			var r = AccessProvider(name);
			var cn = r.Provider.CreateConnection();
			cn.ConnectionString = r.ConnectionString;
			return cn;
		}

		public static DbProviderFactory GetProviderByConnectionName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);

			return AccessProvider(name)
				.Provider;
		}

		public static string GetProviderName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);

			return AccessProvider(name)
				.ProviderName;
		}

		/// <summary>
		///   Clears cached providers.
		/// </summary>
		public static void ResetProviders()
		{
			__providers = new ConcurrentDictionary<string, ProviderRecord>();
		}

		internal static TConnection CreateConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new()
		{
			var connectionString = AccessProvider(connectionName)
				.ConnectionString;
			var cn = new TConnection();
			cn.ConnectionString = connectionString;
			return cn;
		}

		static ProviderRecord AccessProvider(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);

			return __providers.GetOrAdd(name, (n) =>
			{
				var css = ConfigurationManager.ConnectionStrings[name];
				if (css == null)
				{
					throw new ConfigurationErrorsException(String.Concat("Connection string not found: ", name));
				}
				var r = new ProviderRecord();
				r.ConnectionName = name;
				r.ConnectionString = css.ConnectionString;
				r.ProviderName = css.ProviderName;
				r.Provider = DbProviderFactories.GetFactory(css.ProviderName);
				return r;
			});
		}

		struct ProviderRecord
		{
			public string ConnectionName;
			public string ConnectionString;
			public DbProviderFactory Provider;
			public string ProviderName;
		}
	}
}