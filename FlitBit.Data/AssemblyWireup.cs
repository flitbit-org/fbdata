#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data.SqlClient;
using FlitBit.Data.SqlServer;
using FlitBit.Wireup;

[assembly: FlitBit.Wireup.Meta.Wireup(typeof(FlitBit.Data.AssemblyWireup))]

namespace FlitBit.Data
{
  /// <summary>
  ///   Wires up this assembly.
  /// </summary>
  public sealed class AssemblyWireup : IWireupCommand
  {
    /// <summary>
    ///   Wires up this assembly.
    /// </summary>
    /// <param name="coordinator"></param>
    public void Execute(IWireupCoordinator coordinator)
    {
      DbProviderHelpers.RegisterHelper<SqlClientFactory, SqlConnection, SqlCommand, SqlDbProviderHelper>();
    }
  }
}