﻿namespace FlitBit.Data.DataModel
{
	/// <summary>
	///   Interface for classes that can resolve references to models of type M
	/// </summary>
	/// <typeparam name="M">model type M</typeparam>
	public interface IDataModelResolver<out M, IK>
	{
		/// <summary>
		///   Gets the identity key this resolver can resolve.
		/// </summary>
		IK IdentityKey { get; }

		/// <summary>
		///   Resolves a referenced model instance.
		/// </summary>
		/// <returns>the referenced model</returns>
		M Resolve();
	}
}