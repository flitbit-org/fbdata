using System;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEntity, MapIndex(IndexBehaviors.Index, "EmailAddress", "VerificationState")]
	public interface IEmailAddress
	{
		[MapColumn(ColumnBehaviors.Synthetic | ColumnBehaviors.Discriminator, 80)]
		string Discriminator { get; }

		[MapColumn(ColumnBehaviors.Identity | ColumnBehaviors.Immutable), MapLiftedColumn(LiftedColumnBehaviors.Identity)]
		int OwnerID { get; }

		[MapColumn(200)]
		string EmailAddress { get; set; }

		[MapColumn]
		EmailVerificationStates VerificationState { get; set; }

		[MapLiftedColumn()]
		DateTime DateUpdated { get; }
	}
}