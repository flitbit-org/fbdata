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

		public IDataModelCommand<TestPerson, TestPerson, DbConnection> CreateCommand
		{
			get { return (IDataModelCommand<TestPerson, TestPerson, DbConnection>) _create.Value; }
		}

		public IDataModelCommand<TestPerson, TestPerson, DbConnection> UpdateCommand
		{
			get { return (IDataModelCommand<TestPerson, TestPerson, DbConnection>) _update.Value; }
		}

		public IDataModelCommand<TestPerson, int, DbConnection> ReadByIdCommand
		{
			get { return (IDataModelCommand<TestPerson, int, DbConnection>) _readByID.Value; }
		}

		public IDataModelCommand<TestPerson, string, DbConnection> ReadByNameCommand
		{
			get { return (IDataModelCommand<TestPerson, string, DbConnection>) _readByName.Value; }
		}

		public class Handback
		{
			public DbCommand Command { get; set; }
		}

		class PersonCreateBinding : DataModelCommand<TestPerson, TestPerson, TestPerson, SqlConnection>
		{
			public override int Execute(IDbContext cx, SqlConnection cn, TestPerson key) { throw new NotImplementedException(); }

			public override IEnumerable<TestPerson> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior,
				TestPerson key) { throw new NotImplementedException(); }

			public override TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, TestPerson key)
			{
				var res = key;
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
					cmd.Parameters.Add(new SqlParameter("@ExternalID", new SqlGuid(key.ExternalID)));
					cmd.Parameters.Add(new SqlParameter("@Name", new SqlString(key.Name)));
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

		class PersonReadBinding : DataModelCommand<TestPerson, int, TestPerson, SqlConnection>
		{
			public override int Execute(IDbContext cx, SqlConnection cn, int key) { throw new NotImplementedException(); }
			public override IEnumerable<TestPerson> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, int key) { throw new NotImplementedException(); }

			public override TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, int key)
			{
				var res = default(TestPerson);
				using (var cmd = cn.CreateCommand("SELECT [ID], [ExternalID], [Name] FROM [Person] WHERE [ID] = @ID"))
				{
					cmd.Parameters.Add(new SqlParameter("@ID", key));
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
							throw new DuplicateObjectException(String.Concat("Person.ID: ", key));
						}
					}
				}
				return res;
			}
		}

		class PersonReadByNameBinding : DataModelCommand<TestPerson, string, TestPerson, SqlConnection>
		{
			public override int Execute(IDbContext cx, SqlConnection cn, string key) { throw new NotImplementedException(); }

			public override IEnumerable<TestPerson> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior,
				string key) { throw new NotImplementedException(); }

			public override TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, string key)
			{
				var res = default(TestPerson);
				using (var cmd = cn.CreateCommand("SELECT [ID], [ExternalID], [Name] FROM [Person] WHERE [Name] = @Name"))
				{
					var parm = new SqlParameter("@Name", SqlDbType.NVarChar, 50);
					parm.Value = key;
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
							throw new DuplicateObjectException(String.Concat("Person.Name: ", key));
						}
					}
				}
				return res;
			}
		}

		class PersonUpdateBinding : DataModelCommand<TestPerson, TestPerson, TestPerson, SqlConnection>
		{
			public override int Execute(IDbContext cx, SqlConnection cn, TestPerson key) { throw new NotImplementedException(); }

			public override IEnumerable<TestPerson> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior,
				TestPerson key) { throw new NotImplementedException(); }

			public override TestPerson ExecuteSingle(IDbContext cx, SqlConnection cn, TestPerson key)
			{
				var res = key;
				using (var cmd = cn.CreateCommand(@"
UPDATE [Person] 
	SET [ExternalID] = @ExternalID
		, [Name] = @Name
WHERE [ID] = @ID
"))
				{
					var parm = new SqlParameter("@ExternalID", SqlDbType.UniqueIdentifier);
					parm.Value = key.ExternalID;
					cmd.Parameters.Add(parm);
					parm = new SqlParameter("@Name", SqlDbType.NVarChar, 50);
					parm.Value = key.Name;
					cmd.Parameters.Add(parm);
					parm = new SqlParameter("@ID", SqlDbType.Int);
					parm.Value = key.ID;
					var updated = cmd.ExecuteNonQuery();
					if (updated > 1)
					{
						throw new DuplicateObjectException(String.Concat("Person.ID: ", key.ID));
					}
					else if (updated == 0)
					{
						throw new ObjectNotFoundException(String.Concat("Person.ID: ", key.ID));
					}
				}
				return res;
			}
		}
	}
}