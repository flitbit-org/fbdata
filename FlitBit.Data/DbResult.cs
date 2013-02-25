#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.Data
{
	public sealed class DbResult<T>
	{
		public DbResult(IDbExecutable exe, T result)
		{
			this.Executable = exe;
			this.Result = result;
		}

		public IDbExecutable Executable { get; private set; }
		public T Result { get; private set; }
	}	
}
