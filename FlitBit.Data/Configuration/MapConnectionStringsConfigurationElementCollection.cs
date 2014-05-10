#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using FlitBit.Core.Configuration;

namespace FlitBit.Data.Configuration
{
	/// <summary>
	/// Collection of configured mapped connection strings.
	/// </summary>
	public class MapConnectionStringsConfigurationElementCollection : AbstractConfigurationElementCollection<MapConnectionStringConfigSection, string>
	{
		/// <summary>
		///   Gets the element's key
		/// </summary>
		/// <param name="element">the element</param>
		/// <returns>the key</returns>
		protected override string PerformGetElementKey(MapConnectionStringConfigSection element)
		{
			return element.FromName;
		}
	}
}