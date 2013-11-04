using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FlitBit.Data.Catalog;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Tests.Catalog.Models
{
	public class IMappedTypeRepository : DataModelRepository<IMappedType, int, IMappedTypeDataModel, SqlConnection>
	{
		public IMappedTypeRepository(Mapping<IMappedType> mapping) : base(mapping)
		{
		}

		protected override IEnumerable<IMappedType> PerformDirectQueryBy<TItemKey>(IDbContext context, string command, Action<DbCommand, TItemKey> binder, TItemKey key)
		{
			throw new NotImplementedException();
		}

		protected override IMappedType PerformDirectReadBy<TItemKey>(IDbContext context, string command, Action<DbCommand, TItemKey> binder, TItemKey key)
		{
			throw new NotImplementedException();
		}

		public override int GetIdentity(IMappedType model)
		{
			return model.ID;
		}
	}
}
