#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.Core;

namespace FlitBit.Data.DataModel
{
  /// <summary>
  ///   Extensions over IDataModelReference.
  /// </summary>
  public static class DataModelReferenceExtensions
  {
    /// <summary>
    ///   Sets a reference's referent.
    /// </summary>
    /// <param name="reference">the reference</param>
    /// <param name="id">an referent's identity key</param>
    /// <typeparam name="TModel">the model type</typeparam>
    /// <typeparam name="TIdentityKey">the identity key type</typeparam>
    /// <returns>the reference, upcast to a strongly typed reference</returns>
    public static IDataModelReference<TModel, TIdentityKey> SetIdentityKey<TModel, TIdentityKey>(
      this IDataModelReference<TModel> reference, TIdentityKey id)
    {
      var referent = reference as IDataModelReferent<TModel, TIdentityKey>;
      if (referent == null)
      {
        if (reference is IDataModelReferent<TModel>)
        {
          throw new InvalidOperationException(String.Concat("Invalid identity key type for ",
            typeof(TModel).GetReadableSimpleName(), ". Identity key type is ",
            reference.IdentityKeyType.GetReadableSimpleName(), "."));
        }
        throw new InvalidOperationException(String.Concat("Unable to set referent for references of type: ",
          reference.GetType().GetReadableFullName(), "."));
      }
      referent.SetIdentityKey(id);
      return (IDataModelReference<TModel, TIdentityKey>)reference;
    }

    /// <summary>
    ///   Sets a reference's referent.
    /// </summary>
    /// <param name="reference">the reference</param>
    /// <param name="model">the referent</param>
    /// <typeparam name="TModel">the model type</typeparam>
    /// <returns>the reference</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IDataModelReference<TModel> SetReferent<TModel>(this IDataModelReference<TModel> reference,
      TModel model)
    {
      var referent = reference as IDataModelReferent<TModel>;
      if (referent == null)
      {
        throw new InvalidOperationException(String.Concat("Unable to set referent for references of type: ",
          reference.GetType().GetReadableFullName(), "."));
      }
      referent.SetReferent(model);
      return reference;
    }
  }
}