#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.Configuration;

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
    /// <exception cref="System.ArgumentNullException">thrown if the connection name is not specified.</exception>
    /// <exception cref="System.Configuration.ConfigurationErrorsException">
    ///   thrown if the connection name is not found in the
    ///   application's config file.
    /// </exception>
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
    /// <exception cref="System.ArgumentNullException">thrown if the connection name is not specified.</exception>
    /// <exception cref="System.ArgumentException">thrown if the connection name an empty string.</exception>
    /// <exception cref="System.Configuration.ConfigurationErrorsException">
    ///   thrown if the connection name is not found in the
    ///   application's config file.
    /// </exception>
    public static DbConnection CreateConnection(string name)
    {
      Contract.Requires<ArgumentNullException>(name != null);
      Contract.Requires<ArgumentException>(name.Length > 0);

      var r = AccessProvider(name);
      var cn = r.Provider.CreateConnection();
      Contract.Assume(cn != null, "Guaranteed by AccessProvider");
      cn.ConnectionString = r.ConnectionString;
      return cn;
    }

    public static DbProviderFactory GetProviderByConnectionName(string name)
    {
      Contract.Requires<ArgumentNullException>(name != null);
      Contract.Requires<ArgumentException>(name.Length > 0);

      return AccessProvider(name)
        .Provider;
    }

    public static string GetProviderName(string name)
    {
      Contract.Requires<ArgumentNullException>(name != null);
      Contract.Requires<ArgumentException>(name.Length > 0);

      return AccessProvider(name)
        .ProviderName;
    }

    /// <summary>
    ///   Clears cached providers.
    /// </summary>
    public static void ResetProviders() { __providers = new ConcurrentDictionary<string, ProviderRecord>(); }

    internal static TConnection CreateConnection<TConnection>(string connectionName)
      where TConnection : DbConnection
    {
      var provider = AccessProvider(connectionName);
      var connectionString = provider
        .ConnectionString;
      var cn = provider.Provider.CreateConnection();
      Contract.Assume(cn != null, "Guaranteed by AccessProvider");
      cn.ConnectionString = connectionString;
      return (TConnection)cn;
    }

    internal static ProviderRecord AccessProvider(string name)
    {
      Contract.Requires<ArgumentNullException>(name != null);
      Contract.Requires<ArgumentException>(name.Length > 0);

      return __providers.GetOrAdd(name, GetProviderRecordFromConfiguration);
    }

    static ProviderRecord GetProviderRecordFromConfiguration(string name)
    {
      var map = DataModelConfigSection.Instance.MapConnectionStrings[name];
      var n = (map != null) ? map.ToName : name;

      var css = ConfigurationManager.ConnectionStrings[n];
      if (css == null)
      {
        throw new ConfigurationErrorsException(String.Concat("Connection string not found: ", name));
      }
      var r = new ProviderRecord
      {
        ConnectionString = css.ConnectionString,
        ProviderName = css.ProviderName,
        Provider = DbProviderFactories.GetFactory(css.ProviderName)
      };
      return r;
    }
  }
  internal struct ProviderRecord
  {
      public string ConnectionString;
      public DbProviderFactory Provider;
      public string ProviderName;
  }
}