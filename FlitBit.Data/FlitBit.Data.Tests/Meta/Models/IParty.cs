using System;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;
using FlitBit.Wireup.Meta;



namespace FlitBit.Data.Tests.Meta.Models
{
	[DataModel(typeof(PreparePartyMapping))]
	public interface IParty
	{
		[IdentityKey]
		int ID { get; }

		string Name { get; set; }

		DateTime DateCreated { get; }

		DateTime DateUpdated { get; }
	}

	public sealed class PreparePartyMapping	: PrepareDataMapping<IParty>
	{
		protected override void PrepareMapping(Mapping<IParty> mapping)
		{
			mapping
					.Column(x => x.ID).WithBehaviors(ColumnBehaviors.Synthetic).End()
					.Column(x => x.Name)
						.WithBehaviors(ColumnBehaviors.AlternateKey)
						.WithVariableLength(200)
						.End()
					.Column(c => c.DateUpdated).WithBehaviors(ColumnBehaviors.TimestampOnUpdate).End()
					.Column(c => c.DateCreated).WithBehaviors(ColumnBehaviors.TimestampOnUpdate | ColumnBehaviors.RevisionConcurrency).End()
					.End();
		}
	}
}