using System;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Tests.Patterns
{
  [MapEntity(EntityBehaviors.MapAllProperties, MappingStrategy.OneClassOneTable)]
  public interface ITestNullableDecimal : IDataModel
  {
    [IdentityKey, MapColumn(ColumnBehaviors.Synthetic)]
    int ID { get; }

    [MapColumn(ColumnBehaviors.Nullable, Precision = 19, Scale = 4)]
    decimal? Value { get; set; }

    [MapColumn]
    DateTime DateCreated { get; set; }
  }
}