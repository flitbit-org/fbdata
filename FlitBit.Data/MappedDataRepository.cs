using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data
{
	public abstract class MappedDataRepository<TModel, Id, TModelImpl, TDbConnection, TModelBinder> :
		AbstractCachingRepository<TModel, Id>
		where TModelImpl : class, TModel, new()
		where TDbConnection : DbConnection, new()
		where TModelBinder : IDataModelBinder<TModel, Id, TDbConnection>
	{
		TModelBinder _binder;
		Mapping<TModel> _mapping;

		protected MappedDataRepository(Mapping<TModel> mapping, TModelBinder binder)
			: base(mapping.ConnectionName)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			_mapping = mapping;
		}

		public override int DeleteMatch<TMatch>(IDbContext context, TMatch match)
		{
			throw new NotImplementedException();

			//var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			//if (!cn.State.HasFlag(ConnectionState.Open))
			//{
			//  cn.Open();
			//}
			//return _binder.MakeDeleteMatchCommand(match)
			//              .Execute(context, cn, match);
		}

		public override IQueryable<TModel> Query()
		{
			throw new NotImplementedException();
		}

		public override IDataModelQueryResult<TModel> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match)
		{
			throw new NotImplementedException();

			//var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			//if (!cn.State.HasFlag(ConnectionState.Open))
			//{
			//  cn.Open();
			//}
			//return _binder.MakeReadMatchCommand(match)
			//              .ExecuteMany(context, cn, behavior, match);
		}

		public override int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)
		{
			throw new NotImplementedException();

			//var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			//if (!cn.State.HasFlag(ConnectionState.Open))
			//{
			//  cn.Open();
			//}
			//return _binder.MakeUpdateMatchCommand(match, update)
			//              .Execute(context, cn, match, update);
		}

		protected override IDataModelQueryResult<TModel> PerformAll(IDbContext context, QueryBehavior behavior)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetAllCommand()
										.ExecuteMany(context, cn, behavior);
		}

		protected override TModel PerformCreate(IDbContext context, TModel model)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}

			return _binder.GetCreateCommand()
										.ExecuteSingle(context, cn, (TModelImpl) model);
		}

		protected override bool PerformDelete(IDbContext context, Id id)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetDeleteCommand()
										.Execute(context, cn, id) == 1;
		}

		protected override TModel PerformRead(IDbContext context, Id id)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetReadCommand()
										.ExecuteSingle(context, cn, id);
		}

		protected override TModel PerformUpdate(IDbContext context, TModel model)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetUpdateCommand()
										.ExecuteSingle(context, cn, (TModelImpl) model);
		}
	}
}