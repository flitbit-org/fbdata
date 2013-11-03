
namespace FlitBit.Data.SqlServer
{
	public class DynamicSql
	{
		public DynamicSql()
		{
		}
		public DynamicSql(string sql)
		{
			Text = sql;
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
	}
}
