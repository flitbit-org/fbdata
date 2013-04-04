using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data
{
	public interface ITranslatedTerm
	{
		string IetfLanguageCode { get; set; }
		string Term { get; set; }
	}

	/// <summary>
	/// Defines a translatable term.
	/// </summary>
	public interface ITranslatableTermDefinition
	{
		/// <summary>
		/// The term's string identity.
		/// </summary>
		[IdentityKey]
		string Term { get; set; }

		/// <summary>
		/// Gets and sets the translatable term's description.
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Number of arguemnts required when interpolating the term.
		/// </summary>
		int RequiredInterpolationArguments { get; set; }

		/// <summary>
		/// Name of the interpolation engine.
		/// </summary>
		string InterpolationEngine { get; set; }

		/// <summary>
		/// Gets and sets the language code of the default translated term.
		/// </summary>
		string DefaultIetfLanguageCode { get; set; }

		/// <summary>
		/// Gets the default translated term.
		/// </summary>
		string DefaultTranslatedTerm { get; set; }
	}

	/// <summary>
	/// Resolves terms for end-user display.
	/// </summary>
	public interface ITermResolver
	{
		/// <summary>
		/// Gets a translated term in the identified language.
		/// </summary>
		/// <param name="ietfLanguageCode">an IETF language code identifying the end-user's UI language</param>
		/// <param name="term">the term's name</param>
		/// <returns>The term if defined; otherwise the missing term.</returns>
		ITranslatedTerm GetTranslatedTerm(string ietfLanguageCode, string term);

		/// <summary>
		/// Gets a translated term and interpolates the value using the arguments provided.
		/// </summary>
		/// <param name="ietfLanguageCode">an IETF language code identifying the end user's UI language</param>
		/// <param name="term">the term's name</param>
		/// <param name="args">arguments used when interpolating the term</param>
		/// <returns>The term if defined; otherwise the missing term.</returns>
		ITranslatedTerm GetTranslatedTermAndInterpolate(string ietfLanguageCode, string term, params object[] args);
	}
}
