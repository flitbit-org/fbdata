using System;
using FlitBit.Core.Factory;
using FlitBit.Core.Meta;

namespace FlitBit.Data
{
	/// <summary>
	///   Interface for working with model references.
	/// </summary>
	/// <typeparam name="M">model type M</typeparam>
	/// <typeparam name="IK">identity key type IK</typeparam>
	[DataModelReferenceFactoryAutoImplement]
	public interface IDataModelReferenceFactory<M>
	{
		/// <summary>
		///   Makes a new reference from a referent.
		/// </summary>
		/// <param name="model">the model/referent</param>
		/// <returns>a reference to the model</returns>
		IDataModelReference<M> MakeFromReferent(M model);

		/// <summary>
		///   Makes a reference from a model's identity key.
		/// </summary>
		/// <param name="id">an identity key</param>
		/// <returns>a reference to an model's identity key</returns>
		IDataModelReference<M, IK> MakeFromReferentID<IK>(IK id);
	}

	public sealed class DataModelReferenceFactoryAutoImplement : AutoImplementedAttribute
	{
		public DataModelReferenceFactoryAutoImplement()
			: base(InstanceScopeKind.OnDemand) { }

		public override bool GetImplementation<T>(IFactory factory, Action<Type, Func<T>> complete)
		{
			var ftype = typeof(DataModelReferenceFactory<,>).MakeGenericType(typeof(T), DataModel<T>.IdentityKey.KeyType);
			complete(ftype, null);
			return true;
		}
	}
}