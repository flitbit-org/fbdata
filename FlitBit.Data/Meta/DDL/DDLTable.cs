#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion
 
using System.Collections.Generic;

namespace FlitBit.Data.Meta.DDL
{
	public class DDLTable: DDLNode
	{
		Dictionary<string, DDLTableColumn> _columns = new Dictionary<string, DDLTableColumn>();

		public DDLTable(DDLNode parent, string name, DDLBehaviors behaviors)
			: base(DDLNodeKind.Table, parent, name, behaviors)
		{
		}
				
		public DDLTableColumn DefineColumn(ColumnMapping col, DDLBehaviors behaviors)
		{
			var child = GetChild<DDLTableColumn>(DDLNodeKind.Column, col.TargetName);
			if (child == null)
			{
				AddChild(child = new DDLTableColumn(this, col, _columns.Count, behaviors));
			}
			return child;
		}			
	}
}
