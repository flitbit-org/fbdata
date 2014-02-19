using System;

namespace FlitBit.Data.Expressions
{
  public class SqlJoinParameterExpression : SqlParameterExpression
  {
    public SqlJoinParameterExpression(string text, Type type)
      : base(SqlExpressionKind.Join, text, type)
    {}

  }
}