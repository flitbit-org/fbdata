using System;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
  public class SqlColumnTranslatedExpression : SqlValueExpression
  {
    public SqlColumnTranslatedExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type)
    { }

    public ColumnMapping Column { get; set; }
  }
}