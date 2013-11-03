using System.Collections.Generic;

namespace FlitBit.Sample.People
{
	public interface IContactInfo
	{
		int ID { get; }
		IParty Party { get; }
		IEnumerable<IPhoneNumber> PhoneNumbers { get; }
	}
}