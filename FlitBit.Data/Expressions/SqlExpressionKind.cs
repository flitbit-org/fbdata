#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.Expressions
{
  /// <summary>
  ///   SQL expression kinds.
  /// </summary>
  public enum SqlExpressionKind
  {
    /// <summary>
    ///   Unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///   Indicates the expression refers to the target object.
    /// </summary>
    Self = 1,

    /// <summary>
    ///   Indicates the expression joins another type to the target object.
    /// </summary>
    Join = 2,

    /// <summary>
    ///   Indicates the expression refers to a joined type.
    /// </summary>
    JoinReference = 3,

    /// <summary>
    ///   Indicates the expression refers to one of the parameterized values.
    /// </summary>
    Parameter = 4,

    /// <summary>
    ///   Indicates the expression refers to a constant value.
    /// </summary>
    Constant = 5,

    /// <summary>
    ///   Indicates the expression refers to a null value.
    /// </summary>
    Null = 6,

    /// <summary>
    ///   Indicates the expression accesses a member of a referenced object (either target object or one of the joined types).
    /// </summary>
    MemberAccess = 7,

    /// <summary>
    ///   Indicates the expression compares two value expressions.
    /// </summary>
    Comparison = 8,

    /// <summary>
    ///   Indicates the expression is a binary AND with a left and right operand
    /// </summary>
    AndAlso = 9,

    /// <summary>
    ///   Indicates the expression is a binary OR with a left and right operand
    /// </summary>
    OrElse = 10,
  }
}