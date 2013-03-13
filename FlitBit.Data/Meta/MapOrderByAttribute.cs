#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

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
			Contract.Requires(columns != null, "columns cannot be null");
			Contract.Requires(columns.Length != 0, "columns cannot be empty");

			this.Columns = columns;
		}

		/// <summary>
		///   List of column/property names on the target object that participate in ordering sets (mapped to Order By clauses when ordering is needed).
		/// </summary>
		public string Columns { get; private set; }

		internal IEnumerable<IndexColumnSpec> GetColumnSpecs(Type typ)
		{
			Contract.Requires(typ != null);

			var result = new List<IndexColumnSpec>();
			var cols = Columns.Split(',');
			foreach (var def in cols)
			{
				var name_order = def.Trim().Split(' ');
				if (name_order.Length == 1)
				{
					result.Add(new IndexColumnSpec {Column = name_order[0]});
				}
				else
				{
					result.Add(new IndexColumnSpec
						{
							Column = name_order[0],
							Order = (IndexOrder) Enum.Parse(typeof(IndexOrder), name_order[1], true)
						});
				}
			}
			return result;
		}
	}
}