using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Tests.Patterns
{
  [MapEntity(EntityBehaviors.Pluralized | EntityBehaviors.DefinedColumnsOnly, MappingStrategy.OneClassOneTable)]
  public interface IDescription
  {
    [IdentityKey, MapColumn("DescriptionID", ColumnBehaviors.Synthetic)]
    int ID { get; }

    [MapColumn("LanguageID")]
    int Language { get; set; }

    [MapColumn(ColumnBehaviors.Nullable, 200)]
    string Name { get; set; }

    [MapColumn(ColumnBehaviors.Nullable, 1000)]
    string ShortDescription { get; set; }

    [MapColumn(ColumnBehaviors.Nullable)]
    string LongDescription { get; set; }

    [MapColumn(ColumnBehaviors.Nullable, 512)]
    string ImagePath { get; set; }
  }
}
