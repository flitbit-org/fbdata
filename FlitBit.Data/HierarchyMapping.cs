#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using FlitBit.Data.DataModel;

namespace FlitBit.Data
{
  /// <summary>
  ///   Maintains a model's hierarchy mappings.
  /// </summary>
  /// <typeparam name="TModel">model type TModel</typeparam>
  public class HierarchyMapping<TModel> : IHierarchyMapping<TModel>, IHierarchyMappings
  {
    readonly List<IMapping> _knownSubtypes = new List<IMapping>();

    #region IHierarchyMapping<M> Members

    /// <summary>
    ///   Notifies the hierarchy of a new subtype of TModel.
    /// </summary>
    /// <typeparam name="TSubModel">sub model type</typeparam>
    /// <param name="mapping">the submodel's mapping.</param>
    public void NotifySubtype<TSubModel>(IMapping<TSubModel> mapping) where TSubModel : TModel
    {
      _knownSubtypes.Add(mapping);
      if (OnChanged != null)
      {
        OnChanged(this, new EventArgs());
      }
    }

    #endregion

    #region IHierarchyMappings Members

    /// <summary>
    ///   Gets a model's known subtype's mappings.
    /// </summary>
    public IEnumerable<IMapping> KnownSubtypes { get { return _knownSubtypes.AsReadOnly(); } }

    #endregion

    public event EventHandler<EventArgs> OnChanged;
  }
}