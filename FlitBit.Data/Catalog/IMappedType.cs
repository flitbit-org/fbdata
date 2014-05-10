#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Catalog
{
  [MapEntity("OrmCatalog", EntityBehaviors.DefinedColumnsOnly, MappingStrategy.OneClassOneTable,
    ConnectionName = "orm-catalog")]
  public interface IMappedType
  {
    [MapColumn(ColumnBehaviors.Synthetic), IdentityKey]
    int ID { get; }

    [MapColumn(ColumnBehaviors.TimestampOnInsert)]
    DateTime DateCreated { get; }

    [MapColumn(ColumnBehaviors.TimestampOnUpdate | ColumnBehaviors.RevisionConcurrency)]
    DateTime DateUpdated { get; }

    [MapColumn(128)]
    string Catalog { get; set; }

    [MapColumn(40)]
    string LatestVersion { get; set; }

    [MapColumn(ColumnBehaviors.Nullable)]
    IMappedType MappedBaseType { get; set; }

    [MapColumn(128)]
    string MappedTable { get; set; }

    [MapColumn(ColumnBehaviors.Immutable, 40)]
    string OriginalVersion { get; set; }

    [MapColumn(128)]
    string ReadObjectName { get; set; }

    [MapCollection("MappedBaseType")]
    IList<IMappedType> RegisteredSubtypes { get; }

    [MapColumn(ColumnBehaviors.AlternateKey)]
    Type RuntimeType { get; set; }

    [MapColumn(128)]
    string Schema { get; set; }

    [MapColumn]
    MappingStrategy Strategy { get; set; }

    [MapColumn]
    bool? Active { get; set; }
  }
}