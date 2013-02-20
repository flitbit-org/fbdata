#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
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
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));

			return provider.DefineCommandOnConnection(connection);
		}

		public static IDbExecutable DefineCommandOnConnection(string connection, string commandText)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			if (provider == null)
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));

			return provider.DefineCommandOnConnection(connection, commandText, CommandType.Text);
		}

		public static IDbExecutable DefineCommandOnConnection(string connection, string commandText, CommandType commandType)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			if (provider == null)
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));

			return provider.DefineCommandOnConnection(connection, commandText, commandType);
		}

		public static IDbExecutable FromCommandText(string commandText)
		{
			return FromCommandText(commandText, CommandType.Text);
		}
		public static IDbExecutable FromCommandText(string commandText, CommandType type)
		{
			return new BasicDbExecutable(commandText, type);
		}
		public static IDbExecutable FromConnectionName(string connection)
		{
			return new BasicDbExecutable(connection);
		}	
	}
}
