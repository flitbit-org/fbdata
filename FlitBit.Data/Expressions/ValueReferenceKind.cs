using System;

namespace FlitBit.Data.Expressions
{
	[Flags]
	public enum ValueReferenceKind
	{
		Unknown = 0,
		Ref = 1,
		Self = Ref | 1 << 1,
		Join = Ref | 1 << 2,
		Param = 1 << 3,
		Constant = 1 << 4,
		Null = Constant | 1 << 5
	}
}