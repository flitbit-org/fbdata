#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;

namespace FlitBit.Data.Meta
{
	public struct SyntheticID : IEquatable<SyntheticID>
	{
		const int AsciiOffsetToDigitZero = 48;
		const int OffsetToLowerCaseA = 49;
		const int OffsetToLowerCaseG = 56;
		const int OffsetToUpperCaseA = 17;
		const int OffsetToUpperCaseG = 24;

		/// <summary>
		///   Empty synthetic ID.
		/// </summary>
		public static readonly SyntheticID Empty = new SyntheticID();

		static readonly int CHashCodeSeed = typeof(SyntheticID).AssemblyQualifiedName.GetHashCode();
		static readonly char[] HexDigits = "0123456789ABCDEF".ToCharArray();

		readonly int _hashcode;
		readonly char[] _value;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="value">Value of the ID</param>
		public SyntheticID(string value)
		{
			Contract.Requires(value != null);
			Contract.Requires(value.Length > 0);
			_value = value.ToCharArray();
			_hashcode = _value.CalculateCombinedHashcode(CHashCodeSeed);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="value">Value of the ID</param>
		public SyntheticID(IEnumerable<Char> value)
		{
			Contract.Requires(value != null);
			Contract.Requires(value.Count() > 0);
			_value = value.ToArray();
			_hashcode = _value.CalculateCombinedHashcode(CHashCodeSeed);
		}

		/// <summary>
		///   Indicates whether the ID is empty.
		/// </summary>
		public bool IsEmpty { get { return _hashcode == 0; } }

		/// <summary>
		///   Indicates whether the ID is valid.
		/// </summary>
		public bool IsValid { get { return IsValidID(_value); } }

		/// <summary>
		///   Determins if the ID is equal to another.
		/// </summary>
		/// <param name="other">the other ID</param>
		/// <returns>
		///   <em>true</em> if this and the other are equal; otherwise <em>false</em>
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is SyntheticID && Equals((SyntheticID) obj);
		}

		/// <summary>
		///   Gets the ID's hashcode.
		/// </summary>
		/// <returns>the ID's hashcode</returns>
		public override int GetHashCode()
		{
			return _hashcode;
		}

		/// <summary>
		///   Gets the string representation of the ID.
		/// </summary>
		/// <returns>string representation of the ID</returns>
		public override string ToString()
		{
			return (IsEmpty) ? String.Empty : new String(_value);
		}

		public static bool operator ==(SyntheticID lhs, SyntheticID rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(SyntheticID lhs, SyntheticID rhs)
		{
			return !lhs.Equals(rhs);
		}

		#region IEquatable<SyntheticID> Members

		/// <summary>
		///   Determins if the ID is equal to another.
		/// </summary>
		/// <param name="other">the other ID</param>
		/// <returns>
		///   <em>true</em> if this and the other are equal; otherwise <em>false</em>
		/// </returns>
		public bool Equals(SyntheticID other)
		{
			return _hashcode == other._hashcode
				&& _value.EqualsOrItemsEqual(other._value);
		}

		#endregion

		/// <summary>
		///   Calculates a check digit for a string of hexidecimal characters.
		/// </summary>
		/// <param name="value">hex digits over which a check digit will be calculated</param>
		/// <returns>a check digit for the given value</returns>
		public static char CalculateCheckDigit(string value)
		{
			Contract.Requires(value != null);
			return CalculateCheckDigit(value.ToCharArray());
		}

		/// <summary>
		///   Calculates a check digit for an array of hexidecimal characters.
		/// </summary>
		/// <param name="value">hex digits over which a check digit will be calculated</param>
		/// <returns>a check digit for the given value</returns>
		public static char CalculateCheckDigit(char[] value)
		{
			Contract.Requires(value != null);
			Contract.Requires(value.Length > 0);

			// Modified Luhn algorithm for base 16 check digit. -Pdc      
			var len = value.Length - 1;
			var sum = 0;
			for (var i = 0; i <= len; i++)
			{
				var digit = (value[len - i] - AsciiOffsetToDigitZero);
				if (digit < 0)
				{
					throw new ArgumentException();
				}

				if (digit < 10)
				{
					sum += (i % 2 == 0) ? (digit << 1) % 0xf : digit;
				}
				else if ((digit >= OffsetToUpperCaseA && digit < OffsetToUpperCaseG)
					|| (digit >= OffsetToLowerCaseA && digit < OffsetToLowerCaseG))
				{
					digit = 9 + (0x0F & digit);
					sum += (i % 2 == 0) ? (digit << 1) % 0xf : digit;
				}
				else
				{
					throw new ArgumentException();
				}
			}
			return HexDigits[0xF - (sum % 0xF)];
		}

		/// <summary>
		///   Determines if a value is a valid identity.
		/// </summary>
		/// <param name="value">the value</param>
		/// <returns>
		///   <em>true</em> if the value is formatted as a valid identity; otherwise <em>false</em>
		/// </returns>
		public static bool IsValidID(string value)
		{
			if (value == null || value.Length == 0)
			{
				return false;
			}
			return IsValidID(value.ToCharArray());
		}

		/// <summary>
		///   Determines if a value is a valid identity.
		/// </summary>
		/// <param name="value">the value</param>
		/// <returns>
		///   <em>true</em> if the value is formatted as a valid identity; otherwise <em>false</em>
		/// </returns>
		public static bool IsValidID(char[] value)
		{
			if (value == null || value.Length == 0)
			{
				return false;
			}

			// Modified Luhn algorithm for base 16 check digit. -Pdc      
			var len = value.Length - 1;
			var sum = 0;
			for (var i = 0; i <= len; i++)
			{
				var digit = (value[len - i] - AsciiOffsetToDigitZero);
				if (digit < 0)
				{
					return false;
				}

				if (digit < 10)
				{
					sum += (i % 2 == 1) ? (digit << 1) % 0xf : digit;
				}
				else if ((digit >= OffsetToUpperCaseA && digit < OffsetToUpperCaseG)
					|| (digit >= OffsetToLowerCaseA && digit < OffsetToLowerCaseG))
				{
					digit = 9 + (0x0F & digit);
					sum += (i % 2 == 1) ? (digit << 1) % 0xf : digit;
				}
				else
				{
					return false; // input contains a non-digit character
				}
			}
			return (sum % 0xF == 0);
		}
	}
}