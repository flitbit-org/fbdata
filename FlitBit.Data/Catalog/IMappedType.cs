using System;
using System.Collections.Generic;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Catalog
{		
	[MapEntity("OrmCatalog", EntityBehaviors.DefinedColumnsOnly)]
	public interface IMappedType
	{
		[MapColumn(ColumnBehaviors.Synthetic)]
		int ID { get; }

		[MapColumn(ColumnBehaviors.AlternateKey)]
		Type RuntimeType { get; set; }

		[MapColumn(ColumnBehaviors.Nullable)]
		IMappedType MappedBaseType { get; set; }

		[MapColumn(128)]
		string Catalog { get; set; }

		[MapColumn(128)]
		string Schema { get; set; }

		[MapColumn(128)]
		string MappedTable { get; set; }

		[MapColumn(128)]
		string ReadObjectName { get; set; }

		[MapColumn]
		MappingStrategy Strategy { get; set; }

		[MapColumn(40)]
		string OriginalVersion { get; set; }

		[MapColumn(40)]
		string LatestVersion { get; set; }

		[MapColumn(ColumnBehaviors.TimestampOnInsert)]
		DateTime DateCreated { get; }

		[MapColumn(ColumnBehaviors.TimestampOnUpdate | ColumnBehaviors.RevisionConcurrency)]
		DateTime DateUpdated { get; }

		[MapCollection(ReferenceBehaviors.Lazy)]
		IEnumerable<IMappedType> RegisteredSubtypes { get; }
	}
}
