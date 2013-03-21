﻿using System.Collections.Generic;

namespace FlitBit.Data
{
	/// <summary>
	/// Maintains a model's hierarchy mappings.
	/// </summary>
	/// <typeparam name="TModel">model type TModel</typeparam>
	public class HierarchyMapping<TModel> : IHierarchyMapping<TModel>, IHierarchyMappings<TModel>
	{
		readonly List<IMapping<TModel>> _knownSubtypes = new List<IMapping<TModel>>();

		#region IHierarchyMapping<M> Members

		/// <summary>
		/// Notifies the hierarchy of a new subtype of TModel.
		/// </summary>
		/// <typeparam name="TSubModel">sub model type</typeparam>
		/// <param name="mapping">the submodel's mapping.</param>
		public void NotifySubtype<TSubModel>(IMapping<TSubModel> mapping) where TSubModel : TModel
		{
			this._knownSubtypes.Add((IMapping<TModel>) mapping);
		}

		#endregion

		#region IHierarchyMappings<M> Members

		/// <summary>
		/// Gets a model's known subtype's mappings.
		/// </summary>
		public IEnumerable<IMapping<TModel>> KnownSubtypes { get { return this._knownSubtypes.AsReadOnly(); } }

		#endregion
	}
}