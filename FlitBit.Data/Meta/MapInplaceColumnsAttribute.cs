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