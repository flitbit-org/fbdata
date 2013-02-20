#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	public static class DbProviderFactoryExtensions
	{
		public static DbConnection CreateConnection(this DbProviderFactory factory
			, string connectionStringName)
		{
			Contract.Requires<ArgumentNullException>(factory != null, "factory cannot be null");
			Contract.Requires(connectionStringName != null, "connectionStringName cannot be null");
			
			// Ensure we can identify the connection string...			
			string cs = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
			if (cs == null)
				throw new ArgumentException(String.Format("Connection string not defined: {0}", connectionStringName));

			var cn = factory.CreateConnection();
			cn.ConnectionString = cs;
			return cn;
		}
	}
}
