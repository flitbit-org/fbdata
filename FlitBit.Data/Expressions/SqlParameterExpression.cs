using System;

namespace FlitBit.Data.Expressions
{
  public class SqlParameterExpression : SqlColumnTranslatedExpression
  {
    public SqlParameterExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type)
    {}
  }
}