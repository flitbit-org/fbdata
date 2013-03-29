using FlitBit.Data.Meta;

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEntity]
	public interface IOrganization : IParty
	{
		[MapColumn(ColumnBehaviors.Nullable)]
		IPerson PrimaryContact { get; set; }

		[MapInplaceColumns("Organization:ContactEmail", "ID")]
		IEmailAddress ContactEmail { get; }
	}
}