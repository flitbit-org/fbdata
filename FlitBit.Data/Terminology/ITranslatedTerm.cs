namespace FlitBit.Data.Terminology
{
	/// <summary>
	/// A language translated term.
	/// </summary>
	public interface ITranslatedTerm
	{
		/// <summary>
		/// The IETF language code of the translation.
		/// </summary>
		string IetfLanguageCode { get; set; }
		/// <summary>
		/// The term's name.
		/// </summary>
		string Term { get; set; }
		/// <summary>
		/// The translated term.
		/// </summary>
		string Translation { get; set; }
	}
}