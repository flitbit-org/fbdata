#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Configuration;

namespace FlitBit.Data.Configuration
{
	/// <summary>
	/// Maps a connection string name to another.
	/// </summary>
	public sealed class MapConnectionStringConfigSection : ConfigurationSection
	{
		const string PropertyToName = "to";
		const string PropertyFromName = "from";

		/// <summary>
		/// The connection string name being mapped to another.
		/// </summary>
		[ConfigurationProperty(PropertyFromName, IsRequired = true)]
		public String FromName { get { return (String)this[PropertyFromName]; } set { this[PropertyFromName] = value; } }
		
		/// <summary>
		/// The connection string name used during this execution.
		/// </summary>
		[ConfigurationProperty(PropertyToName, IsRequired = true)]
		public String ToName { get { return (String)this[PropertyToName]; } set { this[PropertyToName] = value; } }
	}
}