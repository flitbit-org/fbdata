using System;
using System.Data;
using FlitBit.Core;
using FlitBit.Core.Parallel;
namespace FlitBit.Data
{
	public interface IDbContext : IInterrogateDisposable, IParallelShared
	{
		IDbConnection NewOrSharedConnection(string connection);
		IDbConnection NewConnection(string connection);

		T Add<T>(T contextual)
			where T : IDbContextual;

	}
}
