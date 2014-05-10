#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.Data.Expressions
{
  public abstract class SqlBinaryExpression : SqlExpression
  {
    protected SqlBinaryExpression(SqlExpressionKind kind, SqlExpression lhs, SqlExpression rhs)
      : base(kind)
    {
      this.Left = lhs;
      this.Right = rhs;
    }

    public SqlExpression Left { get; private set; }
    public SqlExpression Right { get; private set; }
  }
}