#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.Data.Expressions
{
  /// <summary>
  /// Abstract base class of SQL expressions.
  /// </summary>
  public abstract class SqlExpression
  {
    /// <summary>
    /// Creates a new instance of the specified kind.
    /// </summary>
    /// <param name="nodeType"></param>
    protected SqlExpression(SqlExpressionKind nodeType) { Kind = nodeType; }

    /// <summary>
    /// Indicates the expression's kind.
    /// </summary>
    public SqlExpressionKind Kind { get; private set; }

    /// <summary>
    /// Gets a text representation of the expression.
    /// </summary>
    public string Text
    {
      get
      {
        var writer = new SqlWriter();
        Write(writer);
        return writer.Text;
      }
    }

    /// <summary>
    /// Writes the expression, potentially as part of a larger expression.
    /// </summary>
    /// <param name="writer"></param>
    public abstract void Write(SqlWriter writer);

    /// <summary>
    /// Gets the string representation of the expression.
    /// </summary>
    /// <returns></returns>
    public override string ToString() { return Text; }
  }
}