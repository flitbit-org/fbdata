#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.Meta.DDL
{
	public class DDLBatch : DDLNode
	{
		public DDLBatch(DDLBehaviors behaviors)
			: base(DDLNodeKind.None, null, "", behaviors)
		{}
	}
}