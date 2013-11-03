namespace FlitBit.Sample.People
{
	public interface IPhoneNumberKind
	{
		int ID { get; }
		string Kind { get; set; }
		string Description { get; set; }
	}
}