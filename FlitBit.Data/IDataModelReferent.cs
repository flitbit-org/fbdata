namespace FlitBit.Data
{
	/// <summary>
	/// Interface for establishing a model referent.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	public interface IDataModelReferent<in TModel>
	{
		/// <summary>
		/// Sets the model referent.
		/// </summary>
		/// <param name="referent">the referent</param>
		void SetReferent(TModel referent);
	}

	/// <summary>
	/// Interface for estabishing an identifiable model's referent.
	/// </summary>
	/// <typeparam name="TModel">the model's type</typeparam>
	/// <typeparam name="TIdentityKey">the model's identity key type</typeparam>
	public interface IDataModelReferent<in TModel, in TIdentityKey> : IDataModelReferent<TModel>
	{
		/// <summary>
		/// Sets the referent's identity key.
		/// </summary>
		/// <param name="id">the identity key</param>
		void SetIdentityKey(TIdentityKey id);
	}

}