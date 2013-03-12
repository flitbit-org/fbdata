using System;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

[module: MapConnection("test-data")]

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEntity]
	public interface IPartyKind
	{
		[MapColumn(ColumnBehaviors.Synthetic, 20)]
		string Kind { get; }
		[MapColumn(ColumnBehaviors.Nullable, 400)]		
		string Description { get; set; }					
	}

	[MapEntity(Discriminator = "Parties")]
	[MapIndex(IndexBehaviors.Index, "Name")]
	public interface IParty
	{
		[MapColumn(ColumnBehaviors.Synthetic), IdentityKey]
		int ID { get; }

		[MapColumn(ColumnBehaviors.Discriminator)]
		IPartyKind Kind { get; }

		[MapColumn(200)]
		string Name { get; set; }

		[MapColumn(ColumnBehaviors.TimestampOnInsert)]
		DateTime DateCreated { get; }

		[MapColumn(ColumnBehaviors.TimestampOnUpdate | ColumnBehaviors.RevisionConcurrency)]
		DateTime DateUpdated { get; }
	}

	[MapEnum(EnumBehavior.ReferenceValue)]
	[Flags]
	public enum EmailVerificationStates 
	{	
		Unverified = 0,
		Verified = 1,

		/// <summary>
		/// Indicates the system sent a verification email.
		/// </summary>
		VerificationEmailSent = 1 << 1,
		/// <summary>
		/// Indicates the system did not receive a response from
		/// the verification email in the verification period.
		/// </summary>
		VerificationEmailExpired = 1 << 2,
		/// <summary>
		/// Indicates the system recieved a rejection in response 
		/// tho the verification email.
		/// </summary>
		VerificationEmailRejected = 1 << 3,
	}

	[MapEntity]
	[MapIndex(IndexBehaviors.Index, "EmailAddress", "VerificationState")]
	public interface IEmailAddress 
	{
		[MapColumn(ColumnBehaviors.Identity | ColumnBehaviors.Discriminator, 80)]
		string Discriminator { get; }

		[MapColumn(ColumnBehaviors.Identity | ColumnBehaviors.Immutable)]
		[MapLiftedColumn(LiftedColumnBehaviors.Identity)]
		int OwnerID { get; }

		[MapColumn(200)]
		string EmailAddress { get; set; }

		[MapColumn]
		EmailVerificationStates VerificationState { get; set; }

		[MapLiftedColumn()]
		DateTime DateUpdated { get; }
	}

	[MapEntity(Discriminator="People")]
	public interface IPerson : IParty
	{
		[MapColumn(ColumnBehaviors.Nullable, 40)]
		string FirstName { get; set; }

		[MapColumn(ColumnBehaviors.Nullable, 60)]
		string LastName { get; set; }

		[MapColumn(ColumnBehaviors.Nullable, 100)]
		string MiddleNames { get; set; }

		[MapColumn(ColumnBehaviors.AlternateKey, 20)]
		string ScreenName { get; set; }

		[MapInplaceColumns("Person:PrimaryEmail", "ID")]
		IEmailAddress PrimaryEmail { get; }
	}
	
	[MapEntity(Discriminator="Organizations")]
	public interface IOrganization : IParty
	{
		[MapColumn(ColumnBehaviors.Nullable)]
		IPerson PrimaryContact { get; set; }

		[MapInplaceColumns("Organization:ContactEmail", "ID")]
		IEmailAddress ContactEmail { get; }
	}

	[MapEntity(Discriminator = "Groups")]
	public interface IGroup : IParty
	{
		[MapColumn(ColumnBehaviors.Nullable, 200)]
		string Description { get; set; }
	}	
}
