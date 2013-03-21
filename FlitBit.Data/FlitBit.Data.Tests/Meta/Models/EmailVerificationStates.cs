using System;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Tests.Meta.Models
{
	[MapEnum(EnumBehavior.ReferenceValue), Flags]
	public enum EmailVerificationStates
	{
		Unverified = 0,
		Verified = 1,

		/// <summary>
		///   Indicates the system sent a verification email.
		/// </summary>
		VerificationEmailSent = 1 << 1,

		/// <summary>
		///   Indicates the system did not receive a response from
		///   the verification email in the verification period.
		/// </summary>
		VerificationEmailExpired = 1 << 2,

		/// <summary>
		///   Indicates the system recieved a rejection in response
		///   tho the verification email.
		/// </summary>
		VerificationEmailRejected = 1 << 3,
	}
}