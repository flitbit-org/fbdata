#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
  public class SqlColumnTranslatedExpression : SqlValueExpression
  {
    public SqlColumnTranslatedExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type) { }

    public ColumnMapping Column { get; set; }
  }
}