namespace FlitBit.Data.Terminology
{
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
