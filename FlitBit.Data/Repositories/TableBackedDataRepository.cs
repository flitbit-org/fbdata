using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Repositories
{
    public abstract class TableBackedRepository<TModel, TIdentityKey> : AbstractCachingRepository<TModel, TIdentityKey>
    {
        public TableBackedRepository(string connectionName)
            : base(connectionName)
        {
            Contract.Requires<ArgumentNullException>(connectionName != null);
            Contract.Requires<ArgumentException>(connectionName.Length > 0);
        }

        protected string AllCommand { get; set; }
        protected string DeleteCommand { get; set; }
        protected string InsertCommand { get; set; }
        protected string ReadCommand { get; set; }

        protected abstract void BindDeleteCommand(DbCommand cmd, TIdentityKey id);

        protected abstract void BindInsertCommand(DbCommand cmd, TModel model);
        protected abstract void BindReadCommand(DbCommand cmd, TIdentityKey id);
        protected abstract void BindUpdateCommand(DbCommand cmd, TModel model);
        protected abstract TModel CreateInstance();
        protected abstract string MakeUpdateCommand(TModel model);

        protected abstract void PopulateInstance(IDbContext context, TModel model, IDataRecord reader, object state);

        protected virtual object PrepareAllCommand(IDbContext context, DbConnection cn, DbCommand cmd)
        {
            cmd.CommandText = AllCommand;
            return null;
        }

        protected virtual object PrepareInsertCommand(IDbContext context, DbConnection cn, DbCommand cmd, TModel model)
        {
            cmd.CommandText = InsertCommand;
            BindInsertCommand(cmd, model);
            return null;
        }

        protected virtual object PrepareQueryByCommand<TItemKey>(IDbContext context, DbConnection cn, DbCommand cmd,
            string command, Action<DbCommand, TItemKey> binder, TItemKey key)
        {
            cmd.CommandText = command;
            if (binder != null)
            {
                binder(cmd, key);
            }
            return null;
        }

        protected virtual object PrepareReadByCommand<TItemKey>(IDbContext context, DbConnection cn, DbCommand cmd,
            string command, Action<DbCommand, TItemKey> binder, TItemKey key)
        {
            cmd.CommandText = command;
            if (binder != null)
            {
                binder(cmd, key);
            }
            return null;
        }

        protected virtual object PrepareReadCommand(IDbContext context, DbConnection cn, DbCommand cmd, TIdentityKey id)
        {
            cmd.CommandText = ReadCommand;
            BindReadCommand(cmd, id);
            return null;
        }

        protected virtual object PrepareUpdateCommand(IDbContext context, DbConnection cn, DbCommand cmd, TModel model)
        {
            cmd.CommandText = MakeUpdateCommand(model);
            BindUpdateCommand(cmd, model);
            return null;
        }

        protected override IDataModelQueryResult<TModel> PerformAll(IDbContext context, QueryBehavior behavior)
        {
            var result = new List<TModel>();
            var cn = context.SharedOrNewConnection(ConnectionName);
            using (var cmd = cn.CreateCommand())
            {
                var state = PrepareAllCommand(context, cn, cmd);
                if (!cn.State.HasFlag(ConnectionState.Open))
                {
                    cn.Open();
                }
                context.IncrementQueryCounter(1);
                using (var reader = cmd.ExecuteReader())
                {
                    context.IncrementQueryCounter();
                    while (reader.Read())
                    {
                        var model = CreateInstance();
                        PopulateInstance(context, model, reader, state);
                        result.Add(model);
                    }
                }
            }
            return new DataModelQueryResult<TModel>(behavior, result);
        }

        protected override TModel PerformCreate(IDbContext context, TModel model)
        {
            var res = default(TModel);
            var cn = context.SharedOrNewConnection(ConnectionName);
            if (!cn.State.HasFlag(ConnectionState.Open))
            {
                cn.Open();
            }
            using (var cmd = cn.CreateCommand())
            {
                var state = PrepareInsertCommand(context, cn, cmd, model);
                using (var reader = cmd.ExecuteReader())
                {
                    context.IncrementQueryCounter();
                    if (reader.Read())
                    {
                        res = CreateInstance();
                        PopulateInstance(context, res, reader, state);
                    }
                }
            }
            return res;
        }

        protected override bool PerformDelete(IDbContext context, TIdentityKey id)
        {
            var result = false;
            var cn = context.SharedOrNewConnection(ConnectionName);
            if (!cn.State.HasFlag(ConnectionState.Open))
            {
                cn.Open();
            }
            using (var cmd = cn.CreateCommand())
            {
                PrepareDeleteCommand(context, cn, cmd, id);
                result = (cmd.ExecuteNonQuery() > 0);
                context.IncrementQueryCounter();
            }
            return result;
        }

        protected override IEnumerable<TModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command,
            Action<DbCommand, TItemKey> binder, TItemKey key)
        {
            var result = new List<TModel>();
            var cn = context.SharedOrNewConnection(ConnectionName);
            using (var cmd = cn.CreateCommand())
            {
                var state = PrepareQueryByCommand(context, cn, cmd, command, binder, key);
                if (!cn.State.HasFlag(ConnectionState.Open))
                {
                    cn.Open();
                }
                context.IncrementQueryCounter(1);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var model = CreateInstance();
                        PopulateInstance(context, model, reader, state);
                        result.Add(model);
                    }
                }
            }
            return result;
        }

        protected override TModel PerformDirectReadBy<TItemKey>(IDbContext context, string command,
            Action<DbCommand, TItemKey> binder, TItemKey key)
        {
            var result = default(TModel);
            var cn = context.SharedOrNewConnection(ConnectionName);
            if (!cn.State.HasFlag(ConnectionState.Open))
            {
                cn.Open();
            }
            using (var cmd = cn.CreateCommand())
            {
                var state = PrepareReadByCommand(context, cn, cmd, command, binder, key);
                using (var reader = cmd.ExecuteReader())
                {
                    context.IncrementQueryCounter();
                    if (reader.Read())
                    {
                        result = CreateInstance();
                        PopulateInstance(context, result, reader, state);
                    }
                }
            }
            return result;
        }

        protected override TModel PerformRead(IDbContext context, TIdentityKey id)
        {
            var result = default(TModel);
            var cn = context.SharedOrNewConnection(ConnectionName);
            if (!cn.State.HasFlag(ConnectionState.Open))
            {
                cn.Open();
            }
            using (var cmd = cn.CreateCommand())
            {
                var state = PrepareReadCommand(context, cn, cmd, id);
                context.IncrementQueryCounter();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = CreateInstance();
                        PopulateInstance(context, result, reader, state);
                    }
                }
            }
            return result;
        }

        protected override TModel PerformUpdate(IDbContext context, TModel model)
        {
            var res = default(TModel);
            var cn = context.SharedOrNewConnection(ConnectionName);
            using (var cmd = cn.CreateCommand())
            {
                var state = PrepareUpdateCommand(context, cn, cmd, model);
                if (!cn.State.HasFlag(ConnectionState.Open))
                {
                    cn.Open();
                }
                using (var reader = cmd.ExecuteReader())
                {
                    context.IncrementQueryCounter();
                    if (reader.Read())
                    {
                        res = CreateInstance();
                        PopulateInstance(context, res, reader, state);
                    }
                }
            }
            return res;
        }

        void PrepareDeleteCommand(IDbContext context, DbConnection cn, DbCommand cmd, TIdentityKey id)
        {
            cmd.CommandText = DeleteCommand;
            BindDeleteCommand(cmd, id);
        }
    }
}