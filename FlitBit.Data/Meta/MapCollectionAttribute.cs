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
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class MapCollectionAttribute : Attribute
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public MapCollectionAttribute()
			: this(ReferenceBehaviors.Lazy, null, null, null, null)
		{
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="referencePropertyNames"></param>
		public MapCollectionAttribute(string referencePropertyNames)
			: this(ReferenceBehaviors.Lazy, referencePropertyNames, null, null, null)
		{
		}

    /// <summary>
    /// Creates a new instance with the specified arguments.
    /// </summary>
    /// <param name="referencedPropertyNames"></param>
    /// <param name="localPropertyNames"></param>
    public MapCollectionAttribute(string referencedPropertyNames, string localPropertyNames)
      : this(ReferenceBehaviors.Default, referencedPropertyNames, localPropertyNames, null, null)
    {
    }

	  /// <summary>
	  /// Creates a new instance with the specified arguments.
	  /// </summary>
	  /// <param name="behaviors"></param>
	  /// <param name="referencedPropertyNames"></param>
	  /// <param name="localPropertyNames"></param>
	  public MapCollectionAttribute(ReferenceBehaviors behaviors, string referencedPropertyNames, string localPropertyNames)
      : this(behaviors, referencedPropertyNames, localPropertyNames, null, null)
	  {
	  }

	  /// <summary>
    /// Creates a new instance with the specified arguments.
    /// </summary>
	  /// <param name="referencedPropertyNames"></param>
	  /// <param name="localPropertyNames"></param>
	  /// <param name="joinType"></param>
	  /// <param name="joinPropertyNames"></param>
	  public MapCollectionAttribute(string referencedPropertyNames, string localPropertyNames,
	    Type joinType, string joinPropertyNames) 
      : this(ReferenceBehaviors.Default, referencedPropertyNames, localPropertyNames, joinType, joinPropertyNames)
	  {
	  }

	  /// <summary>
	  /// Creates a new instance with the specified arguments.
	  /// </summary>
	  /// <param name="behaviors"></param>
	  /// <param name="referencedPropertyNames"></param>
	  /// <param name="localPropertyNames"></param>
	  /// <param name="joinType">a join type</param>
	  /// <param name="joinPropertyNames">the property(s) on the join type</param>
	  public MapCollectionAttribute(ReferenceBehaviors behaviors, string referencedPropertyNames, string localPropertyNames, Type joinType, string joinPropertyNames)
		{
			Behaviors = behaviors;
			ReferencedProperties = String.IsNullOrEmpty(referencedPropertyNames) 
				? Enumerable.Empty<string>() 
				: referencedPropertyNames.Split(',').Select(s => s.Trim());
			LocalProperties = String.IsNullOrEmpty(localPropertyNames)
				? Enumerable.Empty<string>()
				: localPropertyNames.Split(',').Select(s => s.Trim());
	    JoinType = joinType;
      JoinProperties = String.IsNullOrEmpty(joinPropertyNames)
        ? Enumerable.Empty<string>()
        : joinPropertyNames.Split(',').Select(s => s.Trim());
		}

    /// <summary>
    /// The name of the collection; taken from the member name if not specified.
    /// </summary>
    public string Name { get; set; }

	  /// <summary>
	  /// The collection's join type. A join type is the type that joins the target type with the reference type, such as in a many-to-many join it is the type that holds the references going in both directions.
	  /// </summary>
	  public Type JoinType { get; set; }

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

    /// <summary>
    /// List of property names on the join type that identify the referenced objects.
    /// </summary>
    public IEnumerable<string> JoinProperties { get; internal set; }
  }
}