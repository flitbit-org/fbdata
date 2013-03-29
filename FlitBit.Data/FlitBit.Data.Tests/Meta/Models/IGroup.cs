using FlitBit.Data.Meta;

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEntity]
	public interface IGroup : IParty
	{
		[MapColumn(ColumnBehaviors.Nullable, 200)]
		string Description { get; set; }
	}
}