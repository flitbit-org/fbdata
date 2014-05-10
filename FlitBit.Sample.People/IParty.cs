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

		ICollection<IGroup> GroupMemberships { get; }
		ICollection<IGroup> GroupCommissions { get; } 
	}
}
