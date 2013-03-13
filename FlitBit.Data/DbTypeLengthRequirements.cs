using System;

namespace FlitBit.Data
{
	[Flags]
	public enum DbTypeLengthRequirements
	{
		None = 0,
		Length = 1,
		Precision = 1 << 1,
		Scale = 1 >> 2,
		OptionalScale = Scale | 1 << 3,
		IndicatedByBrackets = 1 << 4,
		IndicatedByParenthesis = 1 << 5,
		ApproximationMapping = 0x08000000,
		LengthSpecifierMask = Length | Precision | OptionalScale,
	}
}