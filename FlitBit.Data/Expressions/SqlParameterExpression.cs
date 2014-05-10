#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Data.Expressions
{
  public class SqlParameterExpression : SqlColumnTranslatedExpression
  {
    public SqlParameterExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type) { }
  }
}