#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Data
{
	/// <summary>
	///   Enumeration of command behaviors.
	/// </summary>
	[Flags]
	public enum CommandBehaviors
	{
		/// <summary>
		///   Default; indicates none specified.
		/// </summary>
		None = 0,

		/// <summary>
		///   Indicates the command should use a shared connection if available.
		/// </summary>
		ShareConnectionIfAvailable = 1,
	}
}