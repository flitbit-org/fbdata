#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
    public class SqlMemberAccessExpression : SqlValueExpression
    {
        public SqlMemberAccessExpression(string text, Type type,
            ColumnMapping col, SqlValueExpression expr)
            : base(SqlExpressionKind.MemberAccess, text, type)
        {
            this.Column = col;
            this.Expression = expr;
            var item = expr;
            while (item.Kind == SqlExpressionKind.MemberAccess)
            {
                item = ((SqlMemberAccessExpression)item).Expression;
            }
            this.Source = item;
        }

        public ColumnMapping Column { get; set; }

        public SqlValueExpression Expression { get; set; }

        public override void Write(SqlWriter writer)
        {
            this.Expression.Write(writer);
            writer.Append('.');
            base.Write(writer);
        }
    }
}