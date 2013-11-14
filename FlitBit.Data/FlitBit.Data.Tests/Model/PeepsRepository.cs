using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.DataModel;
using FlitBit.Data.Repositories;

namespace FlitBit.Data.Tests.Model
{
	public class PeepsRepository : TableBackedRepository<Peep, int>
	{
		protected static string CCacheKey_Name = String.Concat(CCacheKey, ".Name");

		internal static string __InsertCommandFmt = @"
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

		internal static string __UpdateCommandFmt = @"
DECLARE @generated_timestamp DATETIME
SET @generated_timestamp = GETUTCDATE()
UPDATE [{0}].[Peeps]
SET [Name] = @Name,
	[Description] = @Description,
	[DateUpdated]	= @generated_timestamp
WHERE [ID] = @ID
SELECT 
	[ID],
	[Name],
	[Description],
	[DateCreated],
	[DateUpdated]
FROM [{0}].[Peeps]
WHERE [ID] = @ID";

		internal static string __BaseSelectCommandFmt = @"
SELECT 
	[{0}].[Peeps].[ID],
	[{0}].[Peeps].[Name],
	[{0}].[Peeps].[Description],
	[{0}].[Peeps].[DateCreated],
	[{0}].[Peeps].[DateUpdated]
FROM [{0}].[Peeps]";

		internal static string __ByIDCommandFmt = @"
WHERE [{0}].[Peeps].[ID] = @ID";

		internal static string __ByNameCommandFmt = @"
WHERE [{0}].[Peeps].[Name] = @Name";

		internal static string __DeleteCommandFmt = @"
DELETE FROM [{0}].[Peeps]
WHERE [{0}].[Peeps].[ID] = @ID";

		public PeepsRepository(string cnName, string schemaName)
			: base(cnName)
		{
			this.AllCommand = String.Format(__BaseSelectCommandFmt, schemaName);
			this.InsertCommand = String.Format(__InsertCommandFmt, schemaName);
			this.UpdateCommand = String.Format(__UpdateCommandFmt, schemaName);
			this.ReadCommand = String.Concat(
				String.Format(__BaseSelectCommandFmt, schemaName),
				String.Format(__ByIDCommandFmt, schemaName)
				);
			this.ReadByNameCommand = String.Concat(
				String.Format(__BaseSelectCommandFmt, schemaName),
				String.Format(__ByNameCommandFmt, schemaName)
				);
			this.DeleteCommand = String.Format(__DeleteCommandFmt, schemaName);
		}

		protected string ReadByNameCommand { get; set; }

		protected string UpdateCommand { get; set; }

		public override int GetIdentity(Peep model)
		{
			return model.ID;
		}

		public Peep ReadByName(IDbContext context, string name)
		{
			Contract.Requires<ArgumentNullException>(context != null);

			return ReadBy(context, ReadByNameCommand,
				(cmd, n) =>
				{
					var binder = Helper.MakeParameterBinder(cmd);
					binder.DefineAndBindParameter("Name", n);
				}
				, CCacheKey_Name,
				name
				);
		}

		protected override void BindDeleteCommand(DbCommand cmd, int id)
		{
			var binder = Helper.MakeParameterBinder(cmd);
			binder.DefineAndBindParameter("ID", id);
		}

		protected override void BindInsertCommand(DbCommand cmd, Peep model)
		{
			var binder = Helper.MakeParameterBinder(cmd);

			if (model.Name == null)
			{
				binder.DefineAndSetDbNull<string>("Name");
			}
			else
			{
				binder.DefineAndBindParameter("Name", model.Name);
			}
			if (model.Description == null)
			{
				binder.DefineAndSetDbNull<string>("Description");
			}
			else
			{
				binder.DefineAndBindParameter("Description", model.Description);
			}
		}

		protected override void BindReadCommand(DbCommand cmd, int id)
		{
			var binder = Helper.MakeParameterBinder(cmd);
			binder.DefineAndBindParameter("ID", id);
		}

		protected override void BindUpdateCommand(DbCommand cmd, Peep model)
		{
			var binder = Helper.MakeParameterBinder(cmd);
			binder.DefineAndBindParameter("ID", model.ID);
			BindInsertCommand(cmd, model);
		}

		protected override Peep CreateInstance()
		{
			return new Peep();
		}

		protected override string MakeUpdateCommand(Peep model)
		{
			return UpdateCommand;
		}

		protected override void PopulateInstance(IDbContext context, Peep model, IDataRecord reader, object state)
		{
			model.ID = reader.GetInt32(0);
			model.Name = reader.GetValueOrDefault<string>(1);
			model.Description = reader.GetValueOrDefault<string>(2);
			model.DateCreated = reader.GetValueOrDefault<DateTime>(3);
			model.DateUpdated = reader.GetValueOrDefault<DateTime>(4);
		}
	}
}