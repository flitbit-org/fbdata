namespace FlitBit.Sample.People
{
	public interface IGroupCommission
	{
		IGroup Group { get; set; }
		IParty Party { get; set; }
		ICommissionKind Kind { get; }
	}
}