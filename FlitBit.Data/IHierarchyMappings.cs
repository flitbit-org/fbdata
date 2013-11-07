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