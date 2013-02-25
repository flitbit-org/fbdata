#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Collections.Generic;
using System.Linq;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	/// <summary>
	/// Basic repository interface for performing CRUD for 
	/// model type TModel.
	/// </summary>
	/// <typeparam name="TModel">model type TModel</typeparam>
	/// <typeparam name="Id">model's identity type Id</typeparam>
	public interface IDataRepository<TModel, Id>
	{
		Id GetIdentity(TModel model);
		void Create(IDbContext context, TModel model, Continuation<TModel> continuation);
		void Read(IDbContext context, Id id, Continuation<TModel> continuation);
		void Update(IDbContext context, TModel model, Continuation<TModel> continuation);
		void Delete(IDbContext context, Id id, Continuation<bool> continuation);

		void All(IDbContext context, Continuation<IEnumerable<TModel>> continuation);
		void Match<TMatch>(IDbContext context, TMatch match, Continuation<IEnumerable<TModel>> continuation);
		IQueryable<TModel> Query();
	}
}
