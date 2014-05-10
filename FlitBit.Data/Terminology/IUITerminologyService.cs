#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.Terminology
{
  /// <summary>
  ///   Resolves terms for end-user display.
  /// </summary>
  public interface IUITerminologyService
  {
    /// <summary>
    ///   Determines if the terminology service has definitions for the package provided.
    /// </summary>
    /// <param name="terminologyPackage"></param>
    /// <returns></returns>
    bool HasPackage(string terminologyPackage);

    /// <summary>
    ///   Gets a translated term in the identified language.
    /// </summary>
    /// <param name="terminologyPackage">the term's terminology package</param>
    /// <param name="term">the term's name</param>
    /// <param name="ietfLanguageCode">an IETF language code identifying the end-user's UI language</param>
    /// <returns>The term if defined; otherwise the missing term.</returns>
    ITranslatedTerm GetTranslatedTerm(string terminologyPackage, string term, string ietfLanguageCode);

    /// <summary>
    ///   Gets a translated term and interpolates the value using the arguments provided.
    /// </summary>
    /// <param name="terminologyPackage">the term's terminology package</param>
    /// <param name="term">the term's name</param>
    /// <param name="ietfLanguageCode">an IETF language code identifying the end user's UI language</param>
    /// <param name="args">arguments used when interpolating the term</param>
    /// <returns>The term if defined; otherwise the missing term.</returns>
    ITranslatedTerm GetTranslatedTermAndInterpolate(string terminologyPackage, string term, string ietfLanguageCode,
      params object[] args);

    /// <summary>
    ///   Gets a translable term's definition.
    /// </summary>
    /// <param name="terminologyPackage">the term's terminology package</param>
    /// <param name="term">the term's name</param>
    /// <returns>The term if defined; otherwise the missing term.</returns>
    ITranslatableTermDefinition GetTermDefinition(string terminologyPackage, string term);

    //IEnumerable<ITranslatableTermDefinition> GetPackageDefinitions(string terminologyPackage);
    //IEnumerable<ITranslatableTermDefinition> GetUntranslatedTermDefinitions(string terminologyPackage, string ietfLanguageCode);
    //IEnumerable<ITranslatedTerm> GetTranslatedTerms(string terminologyPackage, string ietfLanguageCode);
  }
}