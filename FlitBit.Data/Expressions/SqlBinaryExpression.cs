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