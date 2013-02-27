using System;

namespace FlitBit.Data
{
	/// <summary>
	/// DbContext behaviors.
	/// </summary>
	[Flags]
	public enum DbContextBehaviors
	{
		/// <summary>
		/// Indicates the default context behavior.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Indicates the context is independent of context flow.
		/// </summary>
		NoContextFlow = 1,
		/// <summary>
		/// Indicates child contexts will not inherit parent's cache. This 
		/// changes the default behavior.
		/// </summary>
		DisableInheritedCache = 1 << 1,
		/// <summary>
		/// Indicates the context cache will be disabled.
		/// </summary>
		DisableCaching = 1 << 2,		 
	}
}
