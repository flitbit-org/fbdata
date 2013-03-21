using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEntity]
	public interface IPartyKind
	{
		[MapColumn(ColumnBehaviors.Synthetic, 20), IdentityKey]
		string Kind { get; }

		[MapColumn(ColumnBehaviors.Nullable, 400)]
		string Description { get; set; }
	}
}