#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.Meta.DDL
{
  public class DDLCatalog : DDLNode
  {
    public DDLCatalog(DDLNode parent, string name, DDLBehaviors behaviors)
      : base(DDLNodeKind.Catalog, parent, name, behaviors) { }
  }
}