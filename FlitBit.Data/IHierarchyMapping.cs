#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.Data.DataModel;

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