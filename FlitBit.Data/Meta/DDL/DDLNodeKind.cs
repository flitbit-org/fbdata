#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.Data.Meta.DDL
{
	public enum DDLNodeKind
	{
		None = 0,
		Catalog = 100,
		Schema = 200,
		Table = 300,
		Column = 310,

		Default = 311,
		PrimaryKey = 312,
		AlternateKey = 313,
		Index = 314,
		ForeignKey = 315,

		View = 400,
		Procedure = 500,
		Function = 600,
	}
}