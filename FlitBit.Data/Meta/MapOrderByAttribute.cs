#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FlitBit.Data.Meta
{
  [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
  public class MapOrderByAttribute : Attribute
  {
    public MapOrderByAttribute() { }

    public MapOrderByAttribute(string columns)
    {
      Contract.Requires<ArgumentNullException>(columns != null);
      Contract.Requires<ArgumentException>(columns.Length != 0);

      this.Columns = columns;
    }

    /// <summary>
    ///   List of column/property names on the target object that participate in ordering sets (mapped to Order By clauses when
    ///   ordering is needed).
    /// </summary>
    public string Columns { get; private set; }

    internal IEnumerable<IndexColumnSpec> GetColumnSpecs(Type typ)
    {
      Contract.Requires<ArgumentNullException>(typ != null);

      var result = new List<IndexColumnSpec>();
      var cols = Columns.Split(',');
      foreach (var def in cols)
      {
        var nameOrder = def.Trim()
                           .Split(' ');
        if (nameOrder.Length == 1)
        {
          result.Add(new IndexColumnSpec
          {
            Column = nameOrder[0]
          });
        }
        else
        {
          result.Add(new IndexColumnSpec
          {
            Column = nameOrder[0],
            Order = (IndexOrder)Enum.Parse(typeof(IndexOrder), nameOrder[1], true)
          });
        }
      }
      return result;
    }
  }
}