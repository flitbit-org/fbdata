#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data.SqlClient;
using FlitBit.Data;
using FlitBit.Data.Catalog;
using FlitBit.Data.SqlServer;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

[assembly: HookWirupCoordinatorTask]
[assembly: Wireup(typeof(FlitBit.Data.AssemblyWireup))]

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

	/// <summary>
	///   Wires this module.
	/// </summary>
	public class HookWirupCoordinatorTask : WireupTaskAttribute
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		public HookWirupCoordinatorTask()
			: base(WireupPhase.BeforeTasks) { }

		/// <summary>
		/// Called by the base class upon execution. Derived classes should
		///               provide an implementation that performs the wireup logic.
		/// </summary>
		protected override void PerformTask(IWireupCoordinator coordinator, Wireup.Recording.WireupContext context)
		{
			// Attach the root container as a wireup observer...
			coordinator.RegisterObserver(StaticCatalog.Observer);
		}
	}
}