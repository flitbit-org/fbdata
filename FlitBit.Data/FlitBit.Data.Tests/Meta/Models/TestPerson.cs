using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Meta.Tests.Models
{
	public class TestPerson
	{
		Lazy<PersonCreateBinding> _create = new Lazy<PersonCreateBinding>(LazyThreadSafetyMode.ExecutionAndPublication);
		Lazy<PersonReadBinding> _readByID = new Lazy<PersonReadBinding>(LazyThreadSafetyMode.ExecutionAndPublication);

		Lazy<PersonReadByNameBinding> _readByName =
			new Lazy<PersonReadByNameBinding>(LazyThreadSafetyMode.ExecutionAndPublication);

		Lazy<PersonCreateBinding> _update = new Lazy<PersonCreateBinding>(LazyThreadSafetyMode.ExecutionAndPublication);

		public int ID { get; private set; }
		public Guid ExternalID { get; set; }
		public string Name { get; set; }
		public IEnumerable<IPhone> PhoneNumbers { get; internal set; }

		public IDataModelQuerySingleCommand<TestPerson, DbConnection, TestPerson> CreateCommand
		{
			get { return (IDataModelQuerySingleCommand<TestPerson,DbConnection, TestPerson>)_create.Value; }
		}

		public IDataModelQuerySingleCommand<TestPerson, DbConnection, TestPerson> UpdateCommand
		{
			get { return (IDataModelQuerySingleCommand<TestPerson, DbConnection, TestPerson>)_update.Value; }
		}

		public IDataModelQuerySingleCommand<TestPerson,DbConnection, int> ReadByIdCommand
		{
			get { return (IDataModelQuerySingleCommand<TestPerson,DbConnection, int>)_readByID.Value; }
		}

		public IDataModelQuerySingleCommand<TestPerson, DbConnection, string> ReadByNameCommand
		{
			get { return (IDataModelQuerySingleCommand<TestPerson, DbConnection, string>)_readByName.Value; }
		}

		public class Handback
		{
			public DbCommand Command { get; set; }
		}

		class PersonCreateBinding : IDataModelQuerySingleCommand<TestPerson, SqlConnection, TestPerson>
		{
			public TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, TestPerson model)
			{
				var res = model;
				using (var cmd = cn.CreateCommand(@"
INSERT INTO [Person]
	(
	[ExternalID],
	[Name]
	)
VALUES
	(
	@ExternalID,
	@Name,
	)
SELECT SCOPE_IDENTITY()
"))
				{
					cmd.Parameters.Add(new SqlParameter("@ExternalID", new SqlGuid(model.ExternalID)));
					cmd.Parameters.Add(new SqlParameter("@Name", new SqlString(model.Name)));
					using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow))
					{
						cx.IncrementQueryCounter();
						if (reader.Read())
						{
							res.ID = reader.GetInt32(0);
						}
					}
				}
				return res;
			}
		}

		class PersonReadBinding : IDataModelQuerySingleCommand<TestPerson, SqlConnection, int>
		{
			public TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, int model)
			{
				var res = default(TestPerson);
				using (var cmd = cn.CreateCommand("SELECT [ID], [ExternalID], [Name] FROM [Person] WHERE [ID] = @ID"))
				{
					cmd.Parameters.Add(new SqlParameter("@ID", model));
					using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow))
					{
						cx.IncrementQueryCounter();
						if (reader.Read())
						{
							res = new TestPerson();
							res.ID = reader.GetInt32(0);
							if (!reader.IsDBNull(1))
							{
								res.ExternalID = reader.GetGuid(1);
							}
							if (!reader.IsDBNull(2))
							{
								res.Name = reader.GetString(2);
							}
						}
						if (reader.Read())
						{
							throw new DuplicateObjectException(String.Concat("Person.ID: ", model));
						}
					}
				}
				return res;
			}
		}

		class PersonReadByNameBinding : IDataModelQuerySingleCommand<TestPerson, SqlConnection, string>
		{
			public TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, string model)
			{
				var res = default(TestPerson);
				using (var cmd = cn.CreateCommand("SELECT [ID], [ExternalID], [Name] FROM [Person] WHERE [Name] = @Name"))
				{
					var parm = new SqlParameter("@Name", SqlDbType.NVarChar, 50);
					parm.Value = model;
					cmd.Parameters.Add(parm);
					using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow))
					{
						cx.IncrementQueryCounter();
						if (reader.Read())
						{
							res = new TestPerson();
							res.ID = reader.GetInt32(0);
							if (!reader.IsDBNull(1))
							{
								res.ExternalID = reader.GetGuid(1);
							}
							if (!reader.IsDBNull(2))
							{
								res.Name = reader.GetString(2);
							}
						}
						if (reader.Read())
						{
							throw new DuplicateObjectException(String.Concat("Person.Name: ", model));
						}
					}
				}
				return res;
			}
		}

		class PersonUpdateBinding : IDataModelQuerySingleCommand<TestPerson, SqlConnection, TestPerson>
		{
			public TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, TestPerson model)
			{
				var res = model;
				using (var cmd = cn.CreateCommand(@"
UPDATE [Person] 
	SET [ExternalID] = @ExternalID
		, [Name] = @Name
WHERE [ID] = @ID
"))
				{
					var parm = new SqlParameter("@ExternalID", SqlDbType.UniqueIdentifier);
					parm.Value = model.ExternalID;
					cmd.Parameters.Add(parm);
					parm = new SqlParameter("@Name", SqlDbType.NVarChar, 50);
					parm.Value = model.Name;
					cmd.Parameters.Add(parm);
					parm = new SqlParameter("@ID", SqlDbType.Int);
					parm.Value = model.ID;
					var updated = cmd.ExecuteNonQuery();
					if (updated > 1)
					{
						throw new DuplicateObjectException(String.Concat("Person.ID: ", model.ID));
					}
					else if (updated == 0)
					{
						throw new ObjectNotFoundException(String.Concat("Person.ID: ", model.ID));
					}
				}
				return res;
			}
		}
	}
}