using System;
using System.Collections.Generic;

namespace FlitBit.Data.Expressions
{
	public class Constraints
	{
		readonly Dictionary<string, Join> _joins = new Dictionary<string, Join>();
		readonly Dictionary<string, Parameter> _parms = new Dictionary<string, Parameter>();
		readonly SqlWriter _builder = new SqlWriter(Environment.NewLine, "  ");

		public Dictionary<string, Join> Joins { get { return this._joins; } }
		public Dictionary<string, Parameter> Parameters { get { return this._parms; } }
		public Condition Conditions { get; set; }
		public SqlWriter Writer { get { return _builder; } }
	}
}