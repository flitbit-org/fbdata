#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Data.Expressions
{
    public class SqlValueExpression : SqlExpression
    {
        readonly string _text;

        public SqlValueExpression(SqlExpressionKind nodeType, string text, Type type)
            : base(nodeType)
        {
            this.Type = type;
            this._text = text;
            this.Source = this;
        }

        public Type Type { get; private set; }

        public SqlValueExpression Source { get; protected set; }

        /// <summary>
        ///     For values mapped onto joins, gets the join.
        /// </summary>
        public SqlJoinExpression Join { get; internal set; }

        public override void Write(SqlWriter writer) { writer.Append(this._text); }
    }
}