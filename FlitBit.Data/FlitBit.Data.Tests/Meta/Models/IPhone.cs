using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlitBit.Data.Meta.Tests.Models
{	
	public interface IPhone
	{
		long ID { get; }

		Person Person { get; set; }
		IPhoneKind Kind { get; set; }
		bool IsPrimaryContactNumber { get; set; }
		string Number { get; set; }
		DateTime DateCreated { get; }
	}
}
