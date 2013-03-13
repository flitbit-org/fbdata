#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using FlitBit.Core;

namespace FlitBit.Data
{
	public struct ParameterBinding : IEquatable<ParameterBinding>
	{
		static readonly int CHashCodeSeed = typeof(ParameterBinding).AssemblyQualifiedName.GetHashCode();

		DbParamDefinition _definition;
		object _specializedValue;

		public DbParamDefinition Definition
		{
			get { return _definition; }
			set { _definition = value; }
		}

		public object SpecializedValue
		{
			get { return _specializedValue; }
			set { _specializedValue = value; }
		}

		public bool Equals(ParameterBinding other)
		{
			return EqualityComparer<DbParamDefinition>.Default.Equals(Definition, other.Definition)
				&& Equals(SpecializedValue, other.SpecializedValue);
		}

		public override bool Equals(object obj)
		{
			return typeof(ParameterBinding).IsInstanceOfType(obj)
				&& Equals((ParameterBinding) obj);
		}

		public override int GetHashCode()
		{
			var prime = Constants.NotSoRandomPrime;
			var result = CHashCodeSeed*prime;
			result ^= _definition.GetHashCode()*prime;
			if (_specializedValue != null)
			{
				result ^= _specializedValue.GetHashCode()*prime;
			}
			return result;
		}

		public static bool operator ==(ParameterBinding lhs, ParameterBinding rhs) { return lhs.Equals(rhs); }

		public static bool operator !=(ParameterBinding lhs, ParameterBinding rhs) { return !lhs.Equals(rhs); }
	}
}