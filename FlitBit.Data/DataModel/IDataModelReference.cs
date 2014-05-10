#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	///   Base interface for model references.
	/// </summary>
	/// <typeparam name="TModel">model type M</typeparam>
	public interface IDataModelReference<out TModel> : ICloneable
	{
		/// <summary>
		/// Indicates whether an exception occurred while resolving the reference.
		/// </summary>
		/// <remarks>
		/// If the reference is faulted the <see cref="Exception"/> property will contain the
		/// exception raised while trying to resolve the reference.
		/// </remarks>
		bool IsFaulted { get; }

		/// <summary>
		/// Gets the exception thrown while resolving the reference if one occurred; otherwise null.
		/// </summary>
		Exception Exception { get; }

		/// <summary>
		///   Indicates whether the reference has a model.
		/// </summary>
		bool HasModel { get; }

		/// <summary>
		/// Indicates whether the reference has an identity key.
		/// </summary>
		bool HasIdentityKey { get; }

		/// <summary>
		/// Gets the referenced model's key type.
		/// </summary>
		Type IdentityKeyType { get; }

		/// <summary>
		///   Indicates whether the reference is empty.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		///   Gets the referenced model.
		/// </summary>
		TModel Model { get; }
	}

	/// <summary>
	///   A model reference.
	/// </summary>
	/// <typeparam name="TModel">model type M</typeparam>
	/// <typeparam name="TIdentityKey">identity key type IK</typeparam>
	public interface IDataModelReference<out TModel, out TIdentityKey> : IDataModelReference<TModel>
	{			 
		/// <summary>
		///   Gets the referenced model's identity key.
		/// </summary>
		TIdentityKey IdentityKey { get; }
	}
}