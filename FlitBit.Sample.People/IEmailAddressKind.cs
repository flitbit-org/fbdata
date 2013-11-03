namespace FlitBit.Sample.People
{
	public interface IEmailAddressKind
	{
		int ID { get; }
		string Kind { get; set; }
		string Description { get; set; }
	}
}