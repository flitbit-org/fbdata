#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FlitBit.Data.Meta.DDL
{
	public class DDLSchema: DDLNode
	{			
		public DDLSchema(DDLNode parent, string name, DDLBehaviors behaviors)
			: base(DDLNodeKind.Schema, parent, name, behaviors)
		{
		}	
	}

}
