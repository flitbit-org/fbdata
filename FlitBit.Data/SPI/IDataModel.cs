#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using FlitBit.Core.Collections;

namespace FlitBit.Data.SPI
{
	/// <summary>
	///   DataModel service provider interface.
	/// </summary>
	public interface IDataModel : ICloneable, INotifyPropertyChanged, IValidatableObject
	{
		/// <summary>
		/// Gets a referent's identity, without resolving the reference.
		/// </summary>
		/// <param name="member">the referent's name</param>
		/// <typeparam name="TIdentityKey">the referent's identity key type.</typeparam>
		/// <returns>the referent's identity</returns>
		/// <exception cref="ArgumentOutOfRangeException">thrown if there is no referent by the given name.</exception>
		TIdentityKey GetReferentID<TIdentityKey>(string member);

		/// <summary>
		/// Sets a referent's identity.
		/// </summary>
		/// <param name="name">the referent's name</param>
		/// <param name="referentID">the referent's identity.</param>
		/// <typeparam name="TIdentityKey">the referent's identity key type.</typeparam>
		/// <returns>the referent's identity</returns>
		/// <exception cref="ArgumentOutOfRangeException">thrown if there is no referent by the given name.</exception>
		void SetReferentID<TIdentityKey>(string name, TIdentityKey referentID);

		/// <summary>
		///   Gets the object's dirty flags.
		/// </summary>
		/// <returns></returns>
		BitVector GetDirtyFlags();

		/// <summary>
		///   Indicates whether the identified member has been updated.
		/// </summary>
		/// <param name="member">the member's name</param>
		/// <returns>
		///   <em>true</em> if the member has been updated; otherwise <em>false</em>.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">thrown if there is no member by the given name.</exception>
		bool IsDirty(string member);

		/// <summary>
		///   Resets all dirty flags, effectively marking the instance as clean.
		/// </summary>
		void ResetDirtyFlags();

		/// <summary>
		/// Used by the framework to load an instance's internal state from a data reader's current row, using the column offsets provided.
		/// </summary>
		/// <param name="reader">a data reader positioned on a row of data</param>
		/// <param name="offsets">column offsets, numbered in the column ordinal order.</param>
		void LoadFromDataReader(DbDataReader reader, int[] offsets);
	}
}