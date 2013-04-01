using System.Collections.Generic;
using FlitBit.Data.DataModel;

namespace FlitBit.Data
{
	/// <summary>
	/// Maintains a model's hierarchy mapping.
	/// </summary>
	/// <typeparam name="TModel">model type TModel</typeparam>
	public interface IHierarchyMappings<out TModel>
	{
		/// <summary>
		/// Gets a model's known subtype's mappings.
		/// </summary>
		IEnumerable<IMapping<TModel>> KnownSubtypes { get; }
	}
}