#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using FlitBit.Core;

namespace FlitBit.Data.Terminology
{
	/// <summary>
	/// Utility class for resolving terms.
	/// </summary>
	public static class Terms
	{
		/// <summary>
		/// Resolves the identified term.
		/// </summary>
		/// <param name="package">the term's package</param>
		/// <param name="term">the term's name</param>
		/// <returns>a translated term using the current UI culture</returns>
		public static string ResolveTerm(string package, string term)
		{
			Contract.Requires<ArgumentNullException>(package != null);
			Contract.Requires<ArgumentNullException>(term != null);

			var terminology = FactoryProvider.Factory.CreateInstance<IUITerminologyService>();
			if (terminology != null)
			{
				var ietfLanguageTag = CultureInfo.CurrentUICulture.IetfLanguageTag;
				var translated = terminology.GetTranslatedTerm(package, term, ietfLanguageTag);
				if (translated != null)
				{
					return translated.Term;
				}
			}
			return String.Concat("Missing term: ", package, ": ", term);
		}

		/// <summary>
		/// Resolves the identified term and interpolates using the provided arguments.
		/// </summary>
		/// <param name="package">the term's package</param>
		/// <param name="term">the term's name</param>
		/// <param name="arguments"></param>
		/// <returns>a translated term using the current UI culture</returns>
		public static string ResolveAndInterpolateTerm(string package, string term, params object[] arguments)
		{
			Contract.Requires<ArgumentNullException>(package != null);
			Contract.Requires<ArgumentNullException>(term != null);

			var terminology = FactoryProvider.Factory.CreateInstance<IUITerminologyService>();
			if (terminology != null)
			{
				var ietfLanguageTag = CultureInfo.CurrentUICulture.IetfLanguageTag;
				var translated = terminology.GetTranslatedTermAndInterpolate(package, term, ietfLanguageTag, arguments);
				if (translated != null)
				{
					return translated.Term;
				}
			}
			return String.Concat("Missing term: ", package, ": ", term);
		}
	}
}
