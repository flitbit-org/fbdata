
using System.Data;

namespace FlitBit.Data.SqlServer
{
	public class DynamicSql
	{
		public DynamicSql()
			: this(null, CommandType.Text)
		{
		}
		public DynamicSql(string sql) : this(sql, CommandType.Text)
		{
		}
		public DynamicSql(string sql, CommandType cmdType)
		{
			Text = sql;
			CommandType = cmdType;
		}

		public string SyntheticIdentityVar { get; set; }

		public string CalculatedTimestampVar { get; set; }

		public string Text { get; set; }

		public override string ToString()
		{
			return Text;
		}

		public string BindIdentityParameter { get; set; }

		public string BindLimitParameter { get; set; }

		public string BindStartRowParameter { get; set; }

		public CommandType CommandType { get; private set; }
	}
}
