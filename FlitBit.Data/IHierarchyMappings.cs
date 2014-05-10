#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Collections.Generic;
using FlitBit.Data.DataModel;

namespace FlitBit.Data
{
	/// <summary>
	/// Maintains a model's hierarchy mapping.
	/// </summary>
	public interface IHierarchyMappings
	{
		/// <summary>
		/// Gets a model's known subtype's mappings.
		/// </summary>
		IEnumerable<IMapping> KnownSubtypes { get; }
	}
}