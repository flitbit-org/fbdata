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
    #region IWireupCommand Members

    /// <summary>
    ///   Wires up this assembly.
    /// </summary>
    /// <param name="coordinator"></param>
    public void Execute(IWireupCoordinator coordinator)
    {
      DbProviderHelpers.RegisterHelper<SqlClientFactory, SqlConnection, SqlCommand, SqlDbProviderHelper>();
    }

    #endregion
  }
}