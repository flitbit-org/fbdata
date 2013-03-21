#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Data.SqlClient;

namespace FlitBit.Data.SqlServer
{
	[CLSCompliant(false)]
	public class SqlDbExecutable :
		DbExecutable<SqlDbProviderHelper, SqlConnection, SqlCommand, SqlDataReader, SqlParameterBinder>
	{
		public SqlDbExecutable()
			: base()
		{}

		public SqlDbExecutable(string cmdText)
			: base(cmdText)
		{}

		public SqlDbExecutable(string cmdText, CommandType cmdType)
			: base(cmdText, cmdType)
		{}

		public SqlDbExecutable(string cmdText, CommandType cmdType, int cmdTimeout)
			: base(cmdText, cmdType, cmdTimeout)
		{}

		public SqlDbExecutable(string connectionName, string cmdText)
			: base(connectionName, cmdText)
		{}

		public SqlDbExecutable(string connectionName, string cmdText, CommandType cmdType)
			: base(connectionName, cmdText, cmdType)
		{}

		public SqlDbExecutable(string connectionName, string cmdText, CommandType cmdType, int cmdTimeout)
			: base(connectionName, cmdText, cmdType, cmdTimeout)
		{}

		public SqlDbExecutable(string connection, IDbExecutable definition)
			: base(connection, definition)
		{}

		public SqlDbExecutable(SqlConnection connection, IDbExecutable definition)
			: base(connection, definition)
		{}

		public SqlDbExecutable(SqlConnection connection, string cmdText)
			: base(connection, cmdText)
		{}

		public SqlDbExecutable(SqlConnection connection, string cmdText, CommandType cmdType)
			: base(connection, cmdText, cmdType)
		{}

		public SqlDbExecutable(SqlConnection connection, string cmdText, CommandType cmdType, int cmdTimeout)
			: base(connection, cmdText, cmdType, cmdTimeout)
		{}
	}
}