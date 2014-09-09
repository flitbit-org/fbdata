using System;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Tests.Models
{
    [MapEntity(EntityBehaviors.MapAllProperties | EntityBehaviors.Pluralized)]
    public interface IPerson
    {
        [IdentityKey]
        [MapColumn(ColumnBehaviors.Synthetic)]
        int PersonID { get; }

        [MapColumn(ColumnBehaviors.Nullable, 40)]
        string FirstName { get; set; }

        [MapColumn(ColumnBehaviors.Nullable, 40)]
        string LastName { get; set; }

        [MapColumn(ColumnBehaviors.Nullable, 40)]
        string MiddleNames { get; set; }

        [MapColumn(ColumnBehaviors.TimestampOnInsert)]
        DateTime DateCreated { get; }
    }
}