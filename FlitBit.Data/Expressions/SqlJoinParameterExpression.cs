﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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