using System;

namespace FlitBit.Data.Expressions
{
  public class SqlNullExpression : SqlColumnTranslatedExpression
  {
    public SqlNullExpression(Type type)
      : base(SqlExpressionKind.Null, "NULL", type)
    {
    }
  }
}