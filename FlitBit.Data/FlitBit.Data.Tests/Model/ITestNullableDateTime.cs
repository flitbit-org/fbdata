using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Tests.Model
{
  [MapEntity(EntityBehaviors.MapAllProperties, MappingStrategy.OneClassOneTable)]
  public interface ITestNullableDateTime : IDataModel
  {
    [IdentityKey, MapColumn(ColumnBehaviors.Synthetic)]
    int ID { get; }
    [MapColumn(ColumnBehaviors.Nullable)]
    DateTime? TouchedDate { get; set; }
  }
}
