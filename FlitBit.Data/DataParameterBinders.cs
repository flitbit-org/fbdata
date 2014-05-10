#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using FlitBit.Data.SqlServer;

namespace FlitBit.Data
{
  public static class DataParameterBinders
  {
    static readonly ConcurrentDictionary<string, Func<DbCommand, IDataParameterBinder>> __binderFactories =
      new ConcurrentDictionary<string, Func<DbCommand, IDataParameterBinder>>();

    [SuppressMessage("Microsoft.Performance", "CA1810", Justification = "By design.")]
    static DataParameterBinders()
    {
      var key = typeof(SqlCommand).AssemblyQualifiedName;
      __binderFactories.TryAdd(key, cmd => new SqlParameterBinder());
    }

    public static IDataParameterBinder GetBinderForDbCommand(DbCommand cmd)
    {
      var key = cmd.GetType()
                   .AssemblyQualifiedName;
      var factory = default(Func<DbCommand, IDataParameterBinder>);
      if (__binderFactories.TryGetValue(key, out factory))
      {
        return factory(cmd);
      }
      return default(IDataParameterBinder);
    }

    [SuppressMessage("Microsoft.Design", "CA1034", Justification = "By design.")]
    public static class Wireup
    {
      [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
        Justification = "By design.")
      ]
      public static void RegisterBinderFactory<TDbCommand>(Func<DbCommand, IDataParameterBinder> factory)
        where TDbCommand : DbCommand
      {
        var key = typeof(TDbCommand).AssemblyQualifiedName;
        __binderFactories.TryAdd(key, factory);
      }
    }
  }
}