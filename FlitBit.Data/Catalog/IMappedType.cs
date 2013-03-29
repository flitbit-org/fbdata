using System;
using System.Collections.Generic;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Catalog
{
	[MapEntity("OrmCatalog", EntityBehaviors.DefinedColumnsOnly, MappingStrategy.OneClassOneTable)]
	public interface IMappedType
	{
		[MapColumn(128)]
		string Catalog { get; set; }

		[MapColumn(ColumnBehaviors.TimestampOnInsert)]
		DateTime DateCreated { get; }

		[MapColumn(ColumnBehaviors.TimestampOnUpdate | ColumnBehaviors.RevisionConcurrency)]
		DateTime DateUpdated { get; }

		[MapColumn(ColumnBehaviors.Synthetic), IdentityKey]
		int ID { get; }

		[MapColumn(40)]
		string LatestVersion { get; set; }

		[MapColumn(ColumnBehaviors.Nullable)]
		IMappedType MappedBaseType { get; set; }

		[MapColumn(128)]
		string MappedTable { get; set; }

		[MapColumn(40)]
		string OriginalVersion { get; set; }

		[MapColumn(128)]
		string ReadObjectName { get; set; }

		[MapCollection(ReferenceBehaviors.Lazy)]
		IList<IMappedType> RegisteredSubtypes { get; }

		[MapColumn(ColumnBehaviors.AlternateKey)]
		Type RuntimeType { get; set; }

		[MapColumn(128)]
		string Schema { get; set; }

		[MapColumn]
		MappingStrategy Strategy { get; set; }
	}
}