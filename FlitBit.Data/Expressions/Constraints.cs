using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FlitBit.Data.Expressions
{
	public class Constraints
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Dictionary<string, Join> _joins = new Dictionary<string, Join>();
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Dictionary<string, Parameter> _parms = new Dictionary<string, Parameter>();
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly SqlWriter _builder = new SqlWriter(Environment.NewLine, "  ");

		public Dictionary<string, Join> Joins { get { return this._joins; } }
		public Dictionary<string, Parameter> Parameters { get { return this._parms; } }
		public Condition Conditions { get; set; }
		public SqlWriter Writer { get { return _builder; } }

		public IEnumerable<ParameterValueReference> Arguments { get; set; }
	}
}