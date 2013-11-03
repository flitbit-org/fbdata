namespace FlitBit.Sample.People
{
	public interface IGroupMember
	{
		IGroup Group { get; }
		IParty Member { get; set; }
		IParty ConscribedBy { get; set; }
	}
}