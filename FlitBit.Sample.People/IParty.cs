using System;
using System.Collections.Generic;

namespace FlitBit.Sample.People
{
	public interface IParty
	{
		int ID { get; }
		string Name { get; set; }
		DateTime DateCreated { get; set; }
		DateTime DateUpdated { get; set; }

		IEnumerable<IGroup> GroupMemberships { get; }
		IEnumerable<IGroup> GroupCommissions { get; } 
	}
}
