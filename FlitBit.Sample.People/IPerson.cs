namespace FlitBit.Sample.People
{
	public interface IPerson : IParty
	{
		string FirstName { get; set; }
		string LastName { get; set; }
		string MiddleNames { get; set; }
		new string Name { get; }
		IContactInfo ContactInfo { get; set; }
	}
}