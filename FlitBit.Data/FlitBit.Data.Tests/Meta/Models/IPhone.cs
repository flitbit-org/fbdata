using System;

namespace FlitBit.Data.Meta.Tests.Models
{
	public interface IPhone
	{
		long ID { get; }

		TestPerson Person { get; set; }
		IPhoneKind Kind { get; set; }
		bool IsPrimaryContactNumber { get; set; }
		string Number { get; set; }
		DateTime DateCreated { get; }
	}
}