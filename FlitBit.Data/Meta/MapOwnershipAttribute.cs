#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlitBit.Data.Meta
{
	/// <summary>
	///   Defines an ownership relationship between an IRepositoryOwner and
	///   entities.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
	public sealed class MapOwnershipAttribute : Attribute
	{
		public MapOwnershipAttribute() { }

		public MapOwnershipAttribute(EntityOwnershipBehaviors behaviors, params Type[] types)
		{
			this.Behaviors = behaviors;
			this.Types = (types != null) ? types.ToArray() : new Type[0];
		}

		public EntityOwnershipBehaviors Behaviors { get; private set; }
		public IEnumerable<Type> Types { get; private set; }
	}
}