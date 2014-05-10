#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;

namespace FlitBit.Data
{
	public static class DbExecutable
	{
		public static IDbExecutable DefineCommandOnConnection(string connection)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			if (provider == null)
			{
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));
			}

			return provider.DefineExecutableOnConnection(connection);
		}

		public static IDbExecutable DefineCommandOnConnection(string connection, string commandText)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			if (provider == null)
			{
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));
			}

			return provider.DefineExecutableOnConnection(connection, commandText, CommandType.Text);
		}

		public static IDbExecutable DefineCommandOnConnection(string connection, string commandText, CommandType commandType)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			if (provider == null)
			{
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));
			}

			return provider.DefineExecutableOnConnection(connection, commandText, commandType);
		}

		public static IDbExecutable FromCommandText(string cmdText)
		{
			return new DbExecutableDefinition(cmdText);
		}

		public static IDbExecutable FromCommandText(string cmdText, CommandType cmdType)
		{
			return new DbExecutableDefinition(cmdText, cmdType);
		}

		public static IDbExecutable FromCommandText(string cmdText, CommandType cmdType, int cmdTimeout)
		{
			return new DbExecutableDefinition(cmdText, cmdType, cmdTimeout);
		}

		public static IDbExecutable FromConnectionName(string connectionName)
		{
			var res = new DbExecutableDefinition();
			res.ConnectionName = connectionName;
			return res;
		}
	}
}