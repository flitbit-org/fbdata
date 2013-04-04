using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Terminology
{
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
}