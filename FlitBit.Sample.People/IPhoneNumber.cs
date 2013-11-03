namespace FlitBit.Sample.People
{
	public interface IPhoneNumber
	{
		int ID { get; }
		IPhoneNumberKind Kind { get; set; }
		string PhoneNumber { get; set; }
	}
}