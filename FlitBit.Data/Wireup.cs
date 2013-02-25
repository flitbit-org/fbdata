﻿#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Data.SqlClient;
using FlitBit.Data.SqlServer;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

[assembly: Wireup(typeof(FlitBit.Data.WireupThisAssembly))]

namespace FlitBit.Data
{
	/// <summary>
	/// Wires up this assembly.
	/// </summary>
	public sealed class WireupThisAssembly : IWireupCommand
	{
		/// <summary>
		/// Wires up this assembly.
		/// </summary>
		/// <param name="coordinator"></param>
		public void Execute(IWireupCoordinator coordinator)
		{
			DbProviderHelpers.RegisterHelper<SqlClientFactory, SqlConnection, SqlCommand, SqlDbProviderHelper>();
		}
	}
}