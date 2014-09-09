#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace FlitBit.Data.Expressions
{
    public class SqlWriter
    {
        const int DefaultWriteBufferSize = 400;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly StringBuilder _builder;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly string _indentString;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly string _newLine;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _indent;

        public SqlWriter()
            : this(DefaultWriteBufferSize, Environment.NewLine, "\t") { }

        public SqlWriter(string indent)
            : this(DefaultWriteBufferSize, Environment.NewLine, indent) { }

        public SqlWriter(string newLine, string indent)
            : this(DefaultWriteBufferSize, newLine, indent) { }

        public SqlWriter(int bufferSize, string newLine, string indent)
        {
            _builder = new StringBuilder(bufferSize);
            _newLine = newLine;
            _indentString = indent;
        }

        public string Text { get { return _builder.ToString(); } }

        public SqlWriter Indent()
        {
            _indent++;
            return this;
        }

        public SqlWriter Indent(int i)
        {
            Contract.Requires<ArgumentOutOfRangeException>(i >= 0);
            _indent += i;
            return this;
        }

        public SqlWriter Outdent()
        {
            _indent--;
            return this;
        }

        public SqlWriter Append(string text)
        {
            _builder.Append(text);
            return this;
        }

        public SqlWriter Append(char text)
        {
            _builder.Append(text);
            return this;
        }

        public SqlWriter NewLine()
        {
            _builder.Append(_newLine);
            for (var i = 0; i < _indent; i++)
            {
                _builder.Append(_indentString);
            }
            return this;
        }

        public SqlWriter NewLine(string text)
        {
            NewLine();
            return Append(text);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString() { return _builder.ToString(); }
    }
}