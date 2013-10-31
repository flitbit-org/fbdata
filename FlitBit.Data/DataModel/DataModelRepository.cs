using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.DataModel
{
	public abstract class DataModelRepository<TDataModel, TIdentityKey, TModelImpl, TDbConnection> :
		AbstractCachingRepository<TDataModel, TIdentityKey>,
		IDataModelRepository<TDataModel, TIdentityKey, TDbConnection>
		where TModelImpl : class, IDataModel, TDataModel, new()
		where TDbConnection : DbConnection, new() 
	{
		private readonly IDataModelBinder<TDataModel, TIdentityKey, TDbConnection> _binder;
		private readonly Mapping<TDataModel> _mapping;

		protected DataModelRepository(Mapping<TDataModel> mapping)
			: base(mapping.ConnectionName)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(mapping.HasBinder);
			_mapping = mapping;
			_binder = (IDataModelBinder<TDataModel, TIdentityKey, TDbConnection>) mapping.GetBinder();
		}


		public IDataModelBinder<TDataModel, TIdentityKey, TDbConnection> Binder
		{
			get { return _binder; }
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(string queryKey,
			Expression<Func<TDataModel, TParam, bool>> predicate)
		{
			return _binder.MakeQueryCommand<TParam>(queryKey).Where(predicate);
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where<TParam, TParam1>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, bool>> predicate)
		{
			return _binder.MakeQueryCommand<TParam, TParam1>(queryKey).Where(predicate);
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where<TParam, TParam1, TParam2>(
			string queryKey, Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate)
		{
			return Binder.MakeQueryCommand<TParam, TParam1, TParam2>(queryKey).Where(predicate);
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where
			<TParam, TParam1, TParam2, TParam3>(string queryKey,
				Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>> predicate)
		{
			return Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3>(queryKey).Where(predicate);
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where
			<TParam, TParam1, TParam2, TParam3, TParam4>(string queryKey,
				Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>> predicate)
		{
			return Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4>(queryKey).Where(predicate);
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(string queryKey,
				Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>> predicate)
		{
			return Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(queryKey).Where(predicate);
		}

		public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
			Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string queryKey,
				Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>> predicate)
		{
			return
				Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(queryKey).Where(predicate);
		}

		public
			IDataModelQueryCommand
				<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string queryKey,
				Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>> predicate)
		{
			return
				Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(queryKey)
					.Where(predicate);
		}

		public
			IDataModelQueryCommand
				<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Where
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string queryKey,
				Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>>
					predicate)
		{
			return
				Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(queryKey)
					.Where(predicate);
		}

		public
			IDataModelQueryCommand
				<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
			Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string queryKey,
				Expression
					<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>>
					predicate)
		{
			return
				Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
					queryKey).Where(predicate);
		}

		protected override IDataModelQueryResult<TDataModel> PerformAll(IDbContext context, QueryBehavior behavior)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetAllCommand()
				.ExecuteMany(context, cn, behavior);
		}

		protected override TDataModel PerformCreate(IDbContext context, TDataModel model)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}

			return _binder.GetCreateCommand()
				.ExecuteSingle(context, cn, model);
		}

		protected override bool PerformDelete(IDbContext context, TIdentityKey id)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetDeleteCommand()
				.Execute(context, cn, id) == 1;
		}

		protected override TDataModel PerformRead(IDbContext context, TIdentityKey id)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetReadCommand()
				.ExecuteSingle(context, cn, id);
		}

		protected override TDataModel PerformUpdate(IDbContext context, TDataModel model)
		{
			var cn = context.SharedOrNewConnection<TDbConnection>(_mapping.ConnectionName);
			if (!cn.State.HasFlag(ConnectionState.Open))
			{
				cn.Open();
			}
			return _binder.GetUpdateCommand()
				.ExecuteSingle(context, cn, model);
		}
	}
}