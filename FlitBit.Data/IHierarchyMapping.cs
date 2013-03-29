using System;

namespace FlitBit.Data
{
	/// <summary>
	/// Interface for hierarchy mapping.
	/// </summary>
	/// <typeparam name="TModel">model type TModel</typeparam>
	public interface IHierarchyMapping<in TModel>
	{
		event EventHandler<EventArgs> OnChanged;

		/// <summary>
		/// Notifies the hierarchy of a new subtype of TModel.
		/// </summary>
		/// <typeparam name="TSubModel">sub model type</typeparam>
		/// <param name="mapping">the submodel's mapping.</param>
		void NotifySubtype<TSubModel>(IMapping<TSubModel> mapping)
			where TSubModel : TModel;
	}
}