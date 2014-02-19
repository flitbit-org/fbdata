namespace FlitBit.Data.Expressions
{
  public class SqlOrElseExpression : SqlBinaryExpression
  {
    public SqlOrElseExpression(SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.OrElse, lhs, rhs)
    {}

    public override void Write(SqlWriter writer)
    {
      writer.Append("(");
      this.Left.Write(writer);
      writer.Append(" OR ");
      this.Right.Write(writer);
      writer.Append(')');
    }
  }
}