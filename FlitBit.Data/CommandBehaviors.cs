using System;

namespace FlitBit.Data
{
	[Flags]
	public enum CommandBehaviors
	{
		None = 0,
		ShareConnectionIfAvailable = 1,
	}
}
