using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FlitBit.Data.Meta.Tests.Models
{
	public interface IPerson
	{
		int ID { get; }									
		Guid ExternalID { get; set; }
		string Name { get; set; }		 
		IEnumerable<IPhone> PhoneNumbers { get; }
	}

	

	

	
}
