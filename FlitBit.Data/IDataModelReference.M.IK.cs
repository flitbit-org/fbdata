using System;
using FlitBit.Core;

namespace FlitBit.Data
{
	public interface IDataModelReferent<in M>
	{
		void SetReferent(M model);
	}

	public interface IDataModelReferent<in M, IK> : IDataModelReferent<M>
	{
		void SetIdentityKey(IK id);
	}

	/// <summary>
	/// Base interface for model references.
	/// </summary>
	/// <typeparam name="M">model type M</typeparam>
	public interface IDataModelReference<out M> : ICloneable
	{
		/// <summary>
		/// Indicates whether the reference is empty.
		/// </summary>
		bool IsEmpty { get; }
		/// <summary>
		/// Indicates whether the reference has a model.
		/// </summary>
		bool HasModel { get; }
		/// <summary>
		/// Gets the referenced model.
		/// </summary>
		M Model { get; }
	}

	/// <summary>
	/// A model reference.
	/// </summary>
	/// <typeparam name="M">model type M</typeparam>
	/// <typeparam name="IK">identity key type IK</typeparam>
	public interface IDataModelReference<out M, IK> : IDataModelReference<M>
	{
		/// <summary>
		/// Gets the referenced model's identity key.
		/// </summary>
		IK IdentityKey { get; }																										
	}

	public static class IDataModelReferenceExtensions
	{
		public static IDataModelReference<M> SetReferent<M>(this IDataModelReference<M> reference, M model)
		{
			IDataModelReferent<M> referent = (IDataModelReferent<M>)reference;
			referent.SetReferent(model);
			return reference;
		}
		
		public static IDataModelReference<M, IK> SetIdentityKey<M, IK>(this IDataModelReference<M> reference, IK id)
		{
			IDataModelReferent<M, IK> referent = (IDataModelReferent<M, IK>)reference;
			referent.SetIdentityKey(id);
			return (IDataModelReference<M, IK>)reference;
		}	
	}
}
