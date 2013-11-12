#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlitBit.Data.Meta
{
	/// <summary>
	/// Marks a property or field as a mapped collection.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class MapCollectionAttribute : Attribute
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public MapCollectionAttribute()
			: this(ReferenceBehaviors.Lazy, null, null)
		{
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="referencePropertyNames"></param>
		public MapCollectionAttribute(string referencePropertyNames)
			: this(ReferenceBehaviors.Lazy, referencePropertyNames, null)
		{
		}

		/// <summary>
		/// Creates a new instance with the specified arguments.
		/// </summary>
		/// <param name="behaviors"></param>
		/// <param name="referencedPropertyNames"></param>
		/// <param name="localPropertyNames"></param>
		public MapCollectionAttribute(ReferenceBehaviors behaviors, string referencedPropertyNames, string localPropertyNames)
		{
			Behaviors = behaviors;
			ReferencedProperties = String.IsNullOrEmpty(referencedPropertyNames) 
				? Enumerable.Empty<string>() 
				: referencedPropertyNames.Split(',').Select(s => s.Trim());
			LocalProperties = String.IsNullOrEmpty(localPropertyNames)
				? Enumerable.Empty<string>()
				: localPropertyNames.Split(',').Select(s => s.Trim());
		}

		/// <summary>
		/// Indicates the references behaviors.
		/// </summary>
		public ReferenceBehaviors Behaviors { get; private set; }

		/// <summary>
		///   List of property names on the target type that identify the referenced instances.
		/// </summary>
		public IEnumerable<string> ReferencedProperties { get; internal set; }

		/// <summary>
		///   List of property names on the local object that correspond to the referenced properties.
		/// </summary>
		/// <remarks>
		/// If empty the framework assumes the identity key.
		/// </remarks>
		public IEnumerable<string> LocalProperties { get; internal set; }
	}
}