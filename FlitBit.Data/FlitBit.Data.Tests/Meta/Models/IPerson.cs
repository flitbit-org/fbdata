using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEntity]
	public interface IPerson : IParty, IDataModel
	{
		[MapColumn(ColumnBehaviors.Nullable, 40)]
		string FirstName { get; set; }

		[MapColumn(ColumnBehaviors.Nullable, 60)]
		string LastName { get; set; }

		[MapColumn(ColumnBehaviors.Nullable, 100)]
		string MiddleNames { get; set; }

		[MapColumn(ColumnBehaviors.AlternateKey, 20)]
		string ScreenName { get; set; }

		[MapColumn(200)]
		string EmailAddress { get; set; }

		[MapColumn]
		EmailVerificationStates VerificationState { get; set; }

		[MapInplaceColumns("Person:PrimaryEmail", "ID")]
		IEmailAddress PrimaryEmail { get; }
	}
}