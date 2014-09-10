#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace FlitBit.Data.Expressions
{
    /// <summary>
    /// Light weight wrapper over StringBuilder for constructing well formed SQL statements.
    /// </summary>
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

        /// <summary>
        /// Contstructs a new instance.
        /// </summary>
        public SqlWriter()
            : this(DefaultWriteBufferSize, Environment.NewLine, "\t") { }

        /// <summary>
        /// Constructs a new instance using the specified string for intentation.
        /// </summary>
        /// <param name="indent">a string used as indentation</param>
        public SqlWriter(string indent)
            : this(DefaultWriteBufferSize, Environment.NewLine, indent) { }

        /// <summary>
        /// Constructs a new instance using the specified new line and intentation.
        /// </summary>
        /// <param name="newLine">a string used as new line</param>
        /// <param name="indent">a string used as indentation</param>
        public SqlWriter(string newLine, string indent)
            : this(DefaultWriteBufferSize, newLine, indent) { }

        /// <summary>
        /// Constructs a new instance using the specified new line and intentation.
        /// </summary>
        /// <param name="bufferSize">initial buffer size</param>
        /// <param name="newLine">a string used as new line</param>
        /// <param name="indent">a string used as indentation</param>
        public SqlWriter(int bufferSize, string newLine, string indent)
        {
            _builder = new StringBuilder(bufferSize);
            _newLine = newLine;
            _indentString = indent;
        }

        /// <summary>
        /// The sql statement's composed text.
        /// </summary>
        public string Text { get { return _builder.ToString(); } }

        /// <summary>
        /// Indents the statement.
        /// </summary>
        /// <returns></returns>
        public SqlWriter Indent()
        {
            _indent++;
            return this;
        }

        /// <summary>
        /// Indents the statement the specified number of times.
        /// </summary>
        /// <returns></returns>
        public SqlWriter Indent(int i)
        {
            Contract.Requires<ArgumentOutOfRangeException>(i >= 0);
            _indent += i;
            return this;
        }

        /// <summary>
        /// Removes a single indent.
        /// </summary>
        /// <returns></returns>
        public SqlWriter Outdent()
        {
            _indent--;
            return this;
        }

        /// <summary>
        /// Appends the specified text to the sql statement.
        /// </summary>
        /// <param name="text">text</param>
        /// <returns></returns>
        public SqlWriter Append(string text)
        {
            _builder.Append(text);
            return this;
        }

        /// <summary>
        /// Appends the specified char to the sql statement.
        /// </summary>
        /// <param name="text">a char of text</param>
        /// <returns></returns>
        public SqlWriter Append(char text)
        {
            _builder.Append(text);
            return this;
        }

        /// <summary>
        /// Appends the specified value to the sql statement.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlWriter Append(int value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the specified value to the sql statement.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlWriter Append(long value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Adds a new line to the statement, respecting the current indentation level.
        /// </summary>
        /// <returns></returns>
        public SqlWriter NewLine()
        {
            _builder.Append(_newLine);
            for (var i = 0; i < _indent; i++)
            {
                _builder.Append(_indentString);
            }
            return this;
        }

        /// <summary>
        /// Adds a new line to the statement, respecting the current indentation level.
        /// </summary>
        /// <returns></returns>
        public SqlWriter NewLine(string text)
        {
            NewLine();
            return Append(text);
        }

        /// <summary>
        /// Adds a new line to the statement, respecting the current indentation level.
        /// </summary>
        /// <returns></returns>
        public SqlWriter NewLine(char text)
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