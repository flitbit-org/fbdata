namespace FlitBit.Sample.People
{
	public interface IOrganization : IParty
	{
		IContactInfo GeneralContactInfo { get; set; }
		IParty PrimaryContact { get; set; }
		IParty SecondaryContact { get; set; }
	}
}