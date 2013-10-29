using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace FlitBit.Data.Expressions
{
	public class SqlWriter
	{
		private const int DefaultWriteBufferSize = 400;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly StringBuilder _builder;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _indentString;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _newLine;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _indent;

		public SqlWriter()
			: this(DefaultWriteBufferSize, Environment.NewLine, "\t")
		{
		}

		public SqlWriter(string indent)
			: this(DefaultWriteBufferSize, Environment.NewLine, indent)
		{
		}

		public SqlWriter(string newLine, string indent)
			: this(DefaultWriteBufferSize, newLine, indent)
		{
		}

		public SqlWriter(int bufferSize, string newLine, string indent)
		{
			_builder = new StringBuilder(bufferSize);
			_newLine = newLine;
			_indentString = indent;
		}

		public string Text
		{
			get { return _builder.ToString(); }
		}

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
			for (int i = 0; i < _indent; i++)
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
		///   Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		///   A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return _builder.ToString();
		}
	}
}