#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FlitBit.Data.Meta
{
	[Flags]
	public enum IndexBehaviors
	{
		Index = 0,
		Unique = 1,
		Clustered = 2,
		Sparse = 4
	}

	public enum IndexOrder
	{
		Asc = 0,
		Desc = 1
	}

	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
	public sealed class MapIndexAttribute : Attribute
	{
		public MapIndexAttribute()
		{}

		public MapIndexAttribute(IndexBehaviors behaviors, string columns, string include)
		{
			Contract.Requires(columns != null, "columns cannot be null");
			Contract.Requires(columns.Length != 0, "columns cannot be empty");

			this.Behaviors = behaviors;
			this.Columns = columns;
			this.Include = include;
		}

		public MapIndexAttribute(IndexBehaviors behaviors, string columns)
			: this(behaviors, columns, null)
		{}

		public MapIndexAttribute(string columns)
			: this(default(IndexBehaviors), columns, null)
		{}

		public IndexBehaviors Behaviors { get; private set; }

		/// <summary>
		///   List of column/property names on the target object used for referenc.
		/// </summary>
		public string Columns { get; private set; }

		public string Include { get; private set; }

		internal IEnumerable<IndexColumnSpec> GetColumnSpecs(Type typ)
		{
			Contract.Requires(typ != null);

			var result = new List<IndexColumnSpec>();
			var cols = Columns.Split(',');
			foreach (var def in cols)
			{
				var name_order = def.Trim()
														.Split(' ');
				if (name_order.Length == 1)
				{
					result.Add(new IndexColumnSpec
					{
						Column = name_order[0]
					});
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

		internal IEnumerable<string> GetIncludedColumns(Type typ)
		{
			Contract.Requires(typ != null);
			var incl = Include;
			if (String.IsNullOrEmpty(incl))
			{
				return Enumerable.Empty<string>();
			}

			var result = new List<string>();
			foreach (var col in incl.Split(','))
			{
				var name = col.Trim();
				result.Add(name);
			}
			return result;
		}
	}

	public struct IndexColumnSpec
	{
		string _column;
		IndexOrder _order;

		public string Column { get { return _column; } set { _column = value; } }

		public IndexOrder Order { get { return _order; } set { _order = value; } }
	}
}