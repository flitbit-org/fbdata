using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core.Parallel;

namespace FlitBit.Data.Tests.Model
{			
	public class PeepsRepository : TableBackedRepository<Peep, int>
	{					
		public PeepsRepository(string cnName, string schemaName)
			: base(cnName)
		{
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

		static string __UpdateCommandFmt = @"
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

		static string __ByNameCommandFmt = @"
WHERE [{0}].[Peeps].[Name] = @Name";

		static string __DeleteCommandFmt = @"
DELETE FROM [{0}].[Peeps]
WHERE [{0}].[Peeps].[ID] = @ID";

		public override int GetIdentity(Peep model)
		{
			return model.ID;
		}	 		

		protected override string MakeUpdateCommand(Peep model)
		{
			return UpdateCommand;
		}

		protected override Peep CreateInstance() { return new Peep(); }

		public void ReadByName(IDbContext context, string name, Continuation<Peep> continuation)
		{
			ReadBy(context, ReadByNameCommand, binder => binder.DefineAndBindParameter("Name", name), continuation);
		}

		public Peep ReadByName(IDbContext context, string name)
		{
			Contract.Requires<ArgumentNullException>(context != null);

			using (var future = new Future<Peep>())
			{
				ReadByName(context, name, (e, res) =>
				{
					if (e != null) future.MarkFaulted(e);
					else future.MarkCompleted(res);
				});
				return future.Value;
			}
		}

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
			binder.DefineAndBindParameter("ID", model.ID);
			BindInsertCommand(binder, model);			
		}

		protected override void BindDeleteCommand(IDataParameterBinder binder, int id)
		{
			binder.DefineAndBindParameter("ID", id);
		}

		protected string ReadByNameCommand { get; set; }

		protected string UpdateCommand { get; set; }
	}
}
