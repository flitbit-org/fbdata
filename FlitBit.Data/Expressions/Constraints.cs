using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FlitBit.Data.Expressions
{
	public class Constraints
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly Dictionary<string, Parameter> _parameters = new Dictionary<string, Parameter>();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly Dictionary<string, Join> _joins = new Dictionary<string, Join>();

	  public Constraints()
	  {
	    Arguments = new List<ParameterValueReference>();
	    Writer = new SqlWriter(Environment.NewLine, "  ");
	  }

    public IList<Join> Joins { get { return new List<Join>(_joins.Values.OrderBy(j => j.Ordinal)); } }

		public IList<Parameter> Parameters { get { return new List<Parameter>(_parameters.Values.OrderBy(p => p.Ordinal));} }

		public Condition Conditions { get; set; }

		public SqlWriter Writer { get; private set; }

		public IList<ParameterValueReference> Arguments { get; private set; }

	  public bool TryAddParameter(string name, Parameter parm)
	  {
	    Parameter existing;
	    if (_parameters.TryGetValue(name, out existing))
	    {
	      return false;
	    }
	    parm.Ordinal = _parameters.Count;
	    _parameters.Add(name, parm);
	    return true;
	  }

    public bool TryAddJoin(string name, Join @join)
    {
      Join existing;
      if (_joins.TryGetValue(name, out existing))
      {
        return false;
      }
      @join.Ordinal = _parameters.Count;
      _joins.Add(name, @join);
      return true;
    }

    public bool TryGetJoin(string name, out Join @join)
    {
      return _joins.TryGetValue(name, out @join);
    }

    public bool TryGetParameter(string name, out Parameter parm)
    {
      return _parameters.TryGetValue(name, out parm);
    }

	}
}