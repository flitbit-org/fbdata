using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core.Parallel;

namespace FlitBit.Data.Tests.Model
{
	public abstract class TableBackedRepository<TModel, Id> : IDataRepository<TModel, Id>
	{
		public TableBackedRepository(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);
			this.ConnectionName = connectionName;
		}

		public abstract Id GetIdentity(TModel model);

		public void Create(IDbContext context, TModel model, Continuation<TModel> continuation)
		{
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, InsertCommand))
				{
					BindInsertCommand(exe.ParameterBinder, model);

					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(TModel)));
						else
						{
							var reader = res.Result;
							var created = CreateInstance();
							if (reader.Read())
							{
								PopulateInstance(created, res.Executable, reader);
							}
							Go.Parallel(() => continuation(null, created));
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}

		public void Read(IDbContext context, Id id, Continuation<TModel> continuation)
		{
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, ReadCommand))
				{
					BindReadCommand(exe.ParameterBinder, id);

					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(TModel)));
						else
						{
							var reader = res.Result;
							var model = CreateInstance();
							if (reader.Read())
							{
								PopulateInstance(model, res.Executable, reader);
							}
							Go.Parallel(() => continuation(null, model));
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}

		public void Update(IDbContext context, TModel model, Continuation<TModel> continuation)
		{
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				string updateCmd = MakeUpdateCommand(model);
				using (var exe = helper.DefineExecutableOnConnection(cn, updateCmd))
				{
					BindUpdateCommand(exe.ParameterBinder, model);

					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(TModel)));
						else
						{
							var reader = res.Result;
							var updated = CreateInstance();
							if (reader.Read())
							{
								PopulateInstance(updated, res.Executable, reader);
							}
							Go.Parallel(() => continuation(null, updated));
						}
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, default(TModel));
			}
		}			 
		
		public void Delete(IDbContext context, Id id, Continuation<bool> continuation)
		{
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, DeleteCommand))
				{
					BindDeleteCommand(exe.ParameterBinder, id);

					exe.ExecuteNonQuery((e, res) => 
					{							
						if (e != null) Go.Parallel(() => { continuation(e, false); });
						else Go.Parallel(() => continuation(null, (res.Result > 0)));
					});
				}
			}
			catch (Exception e)
			{
				continuation(e, false);
			}
		}

		public void All(IDbContext context, Continuation<IEnumerable<TModel>> continuation)
		{
			try
			{
				var cn = context.SharedOrNewConnection(ConnectionName);
				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				using (var exe = helper.DefineExecutableOnConnection(cn, AllCommand))
				{
					exe.ExecuteReader((e, res) =>
					{
						if (e != null) Go.Parallel(() => continuation(e, default(IEnumerable<TModel>)));
						else
						{
							List<TModel> result = new List<TModel>();
							var reader = res.Result;
							try
							{
								while (reader.Read())
								{
									var model = CreateInstance();
									PopulateInstance(model, exe, reader);
									result.Add(model);
								}
								continuation(null, result);
							}
							catch (Exception ee)
							{
								continuation(ee, default(IEnumerable<TModel>));
							}
						}
					});					
				}
			}
			catch (Exception e)
			{
				continuation(e, default(IEnumerable<TModel>));
			}
		}

		public virtual void Match<TMatch>(IDbContext context, TMatch match, Continuation<IEnumerable<TModel>> continuation)
		{
			throw new NotImplementedException();
		}

		public virtual IQueryable<TModel> Query()
		{
			throw new NotImplementedException();
		}

		protected string ConnectionName { get; private set; }

		protected string InsertCommand { get; set; }
		protected string ReadCommand { get; set; }
		protected string AllCommand { get; set; }
		protected abstract string MakeUpdateCommand(TModel model);
		protected string DeleteCommand { get; set; }
		protected abstract TModel CreateInstance();
		protected abstract void PopulateInstance(TModel model, IDbExecutable exe, IDataRecord reader);
		protected abstract void BindInsertCommand(IDataParameterBinder binder, TModel model);
		protected abstract void BindReadCommand(IDataParameterBinder binder, Id id);
		protected abstract void BindUpdateCommand(IDataParameterBinder binder, TModel model);
		protected abstract void BindDeleteCommand(IDataParameterBinder binder, Id id);
	}

	public class PeepsRepository : TableBackedRepository<Peep, int>
	{					
		public PeepsRepository(string cnName, string schemaName)
			: base(cnName)
		{
			this.InsertCommand = String.Format(__InsertCommandFmt, schemaName);
			this.ReadCommand = String.Concat(
				String.Format(__BaseSelectCommandFmt, schemaName), 
				String.Format(__ByIDCommandFmt, schemaName)
				);
			this.DeleteCommand = String.Concat(String.Concat(__DeleteCommandFmt, schemaName));
		}
						
		static string __InsertCommandFmt = @"
DECLARE @generated_timestamp DATETIME
SET @generated_timestamp = GETUTCDATE()
DECLARE @ID INT
INSERT INTO [{0}].[Peeps]
	(
	[Name],
	[Description],
	[DateCreated],
	[DateUpdated]
	)
VALUES
	(
	@Name,
	@Description,
	@generated_timestamp,
	@generated_timestamp
	)
SET @ID = SCOPE_IDENTITY()
SELECT 
	[ID],
	[Name],
	[Description],
	[DateCreated],
	[DateUpdated]
FROM [{0}].[Peeps]
WHERE [ID] = @ID";

		static string __BaseSelectCommandFmt = @"
SELECT 
	[{0}].[Peeps].[ID],
	[{0}].[Peeps].[Name],
	[{0}].[Peeps].[Description],
	[{0}].[Peeps].[DateCreated],
	[{0}].[Peeps].[DateUpdated]
FROM [{0}].[Peeps]";

		static string __ByIDCommandFmt = @"
WHERE [{0}].[Peeps].[ID] = @ID";

		static string __DeleteCommandFmt = @"
DELETE FROM [{0}].[Peeps]
WHERE [{0}].[Peeps].[ID] = @ID";

		public override int GetIdentity(Peep model)
		{
			return model.ID;
		}

		protected override string MakeUpdateCommand(Peep model)
		{
			throw new NotImplementedException();
		}

		protected override Peep CreateInstance() { return new Peep(); }

		protected override void PopulateInstance(Peep model, IDbExecutable exe, IDataRecord reader)
		{
			model.ID = reader.GetInt32(0);
			model.Name = reader.GetValueOrDefault<string>(1);
			model.Description = reader.GetValueOrDefault<string>(2);
			model.DateCreated = reader.GetValueOrDefault<DateTime>(3);
			model.DateUpdated = reader.GetValueOrDefault<DateTime>(4);

		}

		protected override void BindInsertCommand(IDataParameterBinder binder, Peep model)
		{
			if (model.Name == null) binder.DefineAndSetDbNull<string>("Name");
			else binder.DefineAndBindParameter("Name", model.Name);
			if (model.Description == null) binder.DefineAndSetDbNull<string>("Description");
			else binder.DefineAndBindParameter("Description", model.Description);
		}

		protected override void BindReadCommand(IDataParameterBinder binder, int id)
		{
			binder.DefineAndBindParameter("ID", id);
		}

		protected override void BindUpdateCommand(IDataParameterBinder binder, Peep model)
		{
			throw new NotImplementedException();
		}

		protected override void BindDeleteCommand(IDataParameterBinder binder, int id)
		{
			binder.DefineAndBindParameter("ID", id);
		}
	}
}
