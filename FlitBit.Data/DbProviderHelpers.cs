#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Core;
using FlitBit.Registrar;

namespace FlitBit.Data
{
  /// <summary>
  /// Utilities for accessing DbProviderHelpers.
  /// </summary>
  public static class DbProviderHelpers
  {
    static readonly Registrar<string, Type> __Registry = new Registrar<string, Type>();

    /// <summary>
    /// Gets the DbProviderHelper implementation appropriate for the specified connection.
    /// </summary>
    /// <param name="connection">the connection.</param>
    /// <returns>the helper for the specified connection; if one is not registered then <em>null</em> is returned.</returns>
    public static DbProviderHelper GetDbProviderHelperForDbConnection(DbConnection connection)
    {
      Contract.Requires<ArgumentNullException>(connection != null);

      var key = KeyFor(connection);
      IRegistrationKey<string, Type> registration;
      if (__Registry.TryGetRegistration(key, out registration))
      {
        var res = (DbProviderHelper)Activator.CreateInstance(registration.Handback);
        res.Initialize();
        return res;
      }
      return default(DbProviderHelper);
    }

    /// <summary>
    /// Gets the DbProviderHelper implementation appropriate for the specified DbProviderFactory.
    /// </summary>
    /// <param name="provider">the provider.</param>
    /// <returns>the helper for the specified provider; if one is not registered then <em>null</em> is returned.</returns>
    public static DbProviderHelper GetDbProviderHelperForDbProvider(DbProviderFactory provider)
    {
        Contract.Requires<ArgumentNullException>(provider != null);

        var key = KeyFor(provider);
        IRegistrationKey<string, Type> registration;
        if (__Registry.TryGetRegistration(key, out registration))
        {
            var res = (DbProviderHelper)Activator.CreateInstance(registration.Handback);
            res.Initialize();
            return res;
        }
        return default(DbProviderHelper);
    }

    /// <summary>
    /// Gets the DbProviderHelper implementation appropriate for the specified connection. The connection
    /// must be configured in the application's &lt;connectionStrings>.
    /// </summary>
    /// <param name="connection">the connection's name.</param>
    /// <returns>the helper for the specified connection; if one is not registered then <em>null</em> is returned.</returns>
    public static DbProviderHelper GetDbProviderHelperForDbConnection(string connection)
    {
      Contract.Requires<ArgumentNullException>(connection != null);

      var pr = DbExtensions.GetProviderByConnectionName(connection);
      var key = KeyFor(pr);
      IRegistrationKey<string, Type> registration;
      if (__Registry.TryGetRegistration(key, out registration))
      {
        var res = (DbProviderHelper)Activator.CreateInstance(registration.Handback);
        res.Initialize();
        return res;
      }
      return default(DbProviderHelper);
    }

    /// <summary>
    /// Registers a helper for the DbProvider, DbConnection, and DbCommand specified.
    /// </summary>
    /// <typeparam name="Pr"></typeparam>
    /// <typeparam name="Cn"></typeparam>
    /// <typeparam name="Cm"></typeparam>
    /// <typeparam name="H"></typeparam>
    public static void RegisterHelper<Pr, Cn, Cm, H>()
      where Pr : DbProviderFactory
      where Cn : DbConnection
      where Cm : DbCommand
      where H : DbProviderHelper
    {
      IRegistrationKey<string, Type> registration;
      __Registry.TryRegister(KeyFor<Pr>(), typeof(H), out registration);
      __Registry.TryRegister(KeyFor<Cn>(), typeof(H), out registration);
      __Registry.TryRegister(KeyFor<Cm>(), typeof(H), out registration);
    }

    static string KeyFor<T>() { return typeof(T).AssemblyQualifiedName.InternIt(); }

    static string KeyFor(object instance)
    {
      return instance.GetType()
                     .AssemblyQualifiedName.InternIt();
    }
  }
}