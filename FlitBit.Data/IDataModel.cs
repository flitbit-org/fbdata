using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Core.Collections;

namespace FlitBit.Data.SPI
{
	/// <summary>
	/// DataModel service provider interface; used internally by the framework.
	/// </summary>
	public interface IDataModel : ICloneable
	{
		//TId GetReferentID<TId>(string name);
		//void SetReferentID<TId>(string name, TId referentID);

		/// <summary>
		/// Indicates whether the identified member has been updated.
		/// </summary>
		/// <param name="member">the member's name</param>
		/// <returns><em>true</em> if the member has been updated; otherwise <em>false</em>.</returns>
		bool IsDirty(string member);

		/// <summary>
		/// Gets the object's dirty flags.
		/// </summary>
		/// <returns></returns>
		BitVector GetDirtyFlags();
	}
}
