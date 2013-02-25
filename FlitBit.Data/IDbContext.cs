#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data.Common;
using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public interface IDbContext : IInterrogateDisposable, IParallelShared
	{
		DbConnection SharedOrNewConnection(string connection);
		DbConnection NewConnection(string connection);

		T Add<T>(T item)
			where T : IDisposable;
		
		TConnection SharedOrNewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new();

		TConnection NewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new();

	}
}
