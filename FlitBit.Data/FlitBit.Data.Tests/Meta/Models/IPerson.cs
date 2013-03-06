using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;

namespace FlitBit.Data.Meta.Tests.Models
{
	public class Person
	{
		public int ID { get; private set; }
		public Guid ExternalID { get; set; }
		public string Name { get; set; }
		public IEnumerable<IPhone> PhoneNumbers { get; internal set; }

		public class Handback
		{
			public DbCommand Command { get; set; }
		}

		Lazy<PersonCreateBinding> _create = new Lazy<PersonCreateBinding>(System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		Lazy<PersonCreateBinding> _update = new Lazy<PersonCreateBinding>(System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		Lazy<PersonReadBinding> _readByID = new Lazy<PersonReadBinding>(System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		Lazy<PersonReadByNameBinding> _readByName = new Lazy<PersonReadByNameBinding>(System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

		public IModelCommand<Person, Person, DbConnection> CreateCommand { get { return (IModelCommand<Person, Person, DbConnection>)_create.Value; } }
		public IModelCommand<Person, Person, DbConnection> UpdateCommand { get { return (IModelCommand<Person, Person, DbConnection>)_update.Value; } }
		public IModelCommand<Person, int, DbConnection> ReadByIdCommand { get { return (IModelCommand<Person, int, DbConnection>)_readByID.Value; } }
		public IModelCommand<Person, string, DbConnection> ReadByNameCommand { get { return (IModelCommand<Person, string, DbConnection>)_readByName.Value; } }

		class PersonCreateBinding : ModelCommand<Person, Person, Person, SqlConnection>
		{			 
			public override Person ExecuteSingle(IDbContext cx, SqlConnection cn, Person key)
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
					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess | System.Data.CommandBehavior.SingleRow))
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

			public override IEnumerable<Person> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, Person key)
			{
				throw new NotImplementedException();
			}

			public override int Execute(IDbContext cx, SqlConnection cn, Person key)
			{
				throw new NotImplementedException();
			}
		}

		class PersonUpdateBinding : ModelCommand<Person, Person, Person, SqlConnection>
		{
			public override Person ExecuteSingle(IDbContext cx, SqlConnection cn, Person key)
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
					if (updated > 1) throw new DuplicateObjectException(String.Concat("Person.ID: ", key.ID));
					else if (updated == 0) throw new ObjectNotFoundException(String.Concat("Person.ID: ", key.ID));
				}
				return res;
			}

			public override IEnumerable<Person> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, Person key)
			{
				throw new NotImplementedException();
			}

			
			public override int Execute(IDbContext cx, SqlConnection cn, Person key)
			{
				throw new NotImplementedException();
			}
		}
		
		class PersonReadBinding : ModelCommand<Person, int, Person, SqlConnection>
		{
			public override Person ExecuteSingle(IDbContext cx, SqlConnection cn, int key)
			{
				var res = default(Person);
				using (var cmd = cn.CreateCommand("SELECT [ID], [ExternalID], [Name] FROM [Person] WHERE [ID] = @ID"))
				{
					cmd.Parameters.Add(new SqlParameter("@ID", key));
					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess | System.Data.CommandBehavior.SingleRow))
					{
						cx.IncrementQueryCounter();
						if (reader.Read())
						{
							res = new Person();
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
						if (reader.Read()) throw new DuplicateObjectException(String.Concat("Person.ID: ", key));
					}
				}
				return res;
			}
			public override IEnumerable<Person> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, int key)
			{
				throw new NotImplementedException();
			}

			public override int Execute(IDbContext cx, SqlConnection cn, int key)
			{
				throw new NotImplementedException();
			}
		}

		class PersonReadByNameBinding : ModelCommand<Person, string, Person, SqlConnection>
		{
			public override Person ExecuteSingle(IDbContext cx, SqlConnection cn, string key)
			{
				var res = default(Person);
				using (var cmd = cn.CreateCommand("SELECT [ID], [ExternalID], [Name] FROM [Person] WHERE [Name] = @Name"))
				{
					var parm = new SqlParameter("@Name", SqlDbType.NVarChar, 50);
					parm.Value = key;
					cmd.Parameters.Add(parm);																		 
					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess | System.Data.CommandBehavior.SingleRow))
					{
						cx.IncrementQueryCounter();
						if (reader.Read())
						{
							res = new Person();
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
						if (reader.Read()) throw new DuplicateObjectException(String.Concat("Person.Name: ", key));
					}
				}
				return res;
			}

			public override IEnumerable<Person> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior, string key)
			{
				throw new NotImplementedException();
			}

			public override int Execute(IDbContext cx, SqlConnection cn, string key)
			{
				throw new NotImplementedException();
			}
		}
	}
}
