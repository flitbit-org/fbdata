using FlitBit.Data.Meta;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using AssemblyWireup = FlitBit.Sample.People.AssemblyWireup;

[assembly: Wireup(typeof(AssemblyWireup))]
[module: MapConnection("fb-people")]
[module: MapSchema("people")]

namespace FlitBit.Sample.People
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
      // this is a placeholder.
    }

    #endregion
  }
}