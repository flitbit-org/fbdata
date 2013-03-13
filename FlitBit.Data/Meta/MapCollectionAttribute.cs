#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;

namespace FlitBit.Data.Meta
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class MapCollectionAttribute : Attribute
	{
		public MapCollectionAttribute() { }

		public MapCollectionAttribute(ReferenceBehaviors behaviors, params string[] referencePropertyNames)
		{
			this.ReferenceBehaviors = behaviors;
			this.References = referencePropertyNames;
		}

		public MapCollectionAttribute(params string[] referencePropertyNames) { this.References = referencePropertyNames; }

		/// <summary>
		///   List of column/property names on the target object used for referenc.
		/// </summary>
		public IEnumerable<string> References { get; internal set; }

		public ReferenceBehaviors ReferenceBehaviors { get; private set; }
	}
}