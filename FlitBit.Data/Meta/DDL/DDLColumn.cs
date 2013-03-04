#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;

namespace FlitBit.Data.Meta.DDL
{
	public class DDLTableColumn: DDLNode
	{
		private ColumnMapping _col;
		private int _index;
		
		public DDLTableColumn(DDLTable table, string name, int index, DDLBehaviors behaviors)
			: base(DDLNodeKind.Column, table, name, behaviors)
		{
			_index = index;
		}

		public DDLTableColumn(DDLTable table, ColumnMapping col, int index, DDLBehaviors behaviors)
			: base(DDLNodeKind.Column, table, col.TargetName, behaviors)
		{
			Contract.Requires<ArgumentNullException>(col != null);
			this._col = col;
			this._index = index;
		}

		public DDLTable Table { get { return (DDLTable)Parent; } }

		
	}
}
