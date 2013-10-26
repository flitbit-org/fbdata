using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace FlitBit.Data.Expressions
{
	public class SqlWriter
	{
		const int DefaultWriteBufferSize = 400;
		readonly string _newLine;
		readonly string _indentString;
		readonly StringBuilder _builder;
		int _indent;

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
			this._builder = new StringBuilder(bufferSize);
			this._newLine = newLine;
			this._indentString = indent;
		}

		public SqlWriter Indent()
		{
			this._indent++;
			return this;
		}

		public SqlWriter Indent(int i)
		{
			Contract.Requires<ArgumentOutOfRangeException>(i >= 0);
			this._indent+=i;
			return this;
		}

		public SqlWriter Outdent()
		{
			this._indent--;
			return this;
		}

		public SqlWriter Append(string text)
		{
			this._builder.Append(text);
			return this;
		}

		public SqlWriter Append(char text)
		{
			this._builder.Append(text);
			return this;
		}

		public SqlWriter NewLine()
		{
			this._builder.Append(this._newLine);
			for (var i = 0; i < this._indent; i++)
			{
				this._builder.Append(this._indentString);
			}
			return this;
		}

		public SqlWriter NewLine(string text)
		{
			NewLine();
			return Append(text);
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return this._builder.ToString();
		}
	}
}
