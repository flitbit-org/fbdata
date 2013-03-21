#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Data.Meta
{
	[Flags]
	public enum SyntheticIDBehaviors
	{
		Default = 0,
		LinearCongruentGenerated = 1,
		NibbleValue = 2,
		UseCheckDigit = 4
	}

	public enum SyntheticIDSize
	{
		Undefined = 0,
		Bits_7 = 7,
		Bits_8 = 8,
		Bits_13 = 13,
		Bits_15 = 15,
		Bits_16 = 16,
		Bits_31 = 31,
		Bits_48 = 48,
		Bits_63 = 63,
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class SyntheticIDAttribute : Attribute
	{
		public SyntheticIDAttribute(SyntheticIDBehaviors behaviors)
			: this(behaviors, SyntheticIDSize.Bits_31)
		{}

		public SyntheticIDAttribute(SyntheticIDBehaviors behaviors, SyntheticIDSize size)
		{
			this.Behaviors = behaviors;
			this.Size = size;
		}

		public SyntheticIDBehaviors Behaviors { get; private set; }
		public SyntheticIDSize Size { get; private set; }
	}
}