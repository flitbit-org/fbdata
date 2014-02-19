namespace FlitBit.Data.Expressions
{
  public class SqlAndAlsoExpression : SqlBinaryExpression
  {
    public SqlAndAlsoExpression(SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.AndAlso, lhs, rhs)
    {}

    public override void Write(SqlWriter writer)
    {
      writer.Append("(");
      this.Left.Write(writer);
      writer.Append(" AND ");
      this.Right.Write(writer);
      writer.Append(')');
    }
  }
}