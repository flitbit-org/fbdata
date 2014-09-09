#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Linq;
using System.Reflection;

namespace FlitBit.Data.Meta
{
    public class MapInplaceColumnsAttribute : Attribute
    {
        public MapInplaceColumnsAttribute(string discriminator, string ownerIDColumnName)
        {
            this.Discriminator = discriminator;
            this.OwnerIDColumnName = ownerIDColumnName;
        }

        public string Discriminator { get; set; }

        public string OwnerIDColumnName { get; set; }

        internal void PrepareMapping<T>(Mapping<T> mapping, PropertyInfo prop)
        {
            var contributor = Mappings.AccessMappingFor(prop.PropertyType);
            mapping.AddDependency(contributor, DependencyKind.ColumnContributor, prop);
            foreach (var c in contributor.Columns.Where(c => c.IsIdentity == false))
            {
                mapping.AddContributedColumn(c);
            }
        }
    }
}