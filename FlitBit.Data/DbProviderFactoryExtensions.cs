#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

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
      , string connection)
    {
      Contract.Requires<ArgumentNullException>(factory != null);
      Contract.Requires<ArgumentNullException>(connection != null);

      // Ensure we can identify the connection string...			
      var cs = ConfigurationManager.ConnectionStrings[connection].ConnectionString;
      if (cs == null)
      {
        throw new ArgumentException(String.Format("Connection string not defined: {0}", connection));
      }

      var cn = factory.CreateConnection();
      cn.ConnectionString = cs;
      return cn;
    }
  }
}