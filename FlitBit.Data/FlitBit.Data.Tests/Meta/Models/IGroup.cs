using FlitBit.Data.Meta;

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEntity(Discriminator = "Groups")]
	public interface IGroup : IParty
	{
		[MapColumn(ColumnBehaviors.Nullable, 200)]
		string Description { get; set; }
	}
}