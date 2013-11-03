namespace FlitBit.Sample.People
{
	public interface IGroup : IParty
	{
		string Description { get; set; }
		bool AllowSelfConscribe { get; set; }
	}
}