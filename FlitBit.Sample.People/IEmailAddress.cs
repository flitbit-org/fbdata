namespace FlitBit.Sample.People
{
	public interface IEmailAddress
	{
		int ID { get; }
		IEmailAddressKind Kind { get; set; }
		string EmailAddress { get; set; }
		bool Verified { get; set; }
		bool DoNotContact { get; set; }
	}
}