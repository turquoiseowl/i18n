
namespace i18n
{
    /// <summary>
    /// Describes a service for localizing target messages (aka 'nuggets') that are
    /// embedded in a string.
    /// </summary>
    public interface INuggetLocalizer
    {
        /// <summary>
        /// Method for post-processing the response entity in order to replace any
        /// msgid nuggets such as [[[Translate me!]]] with the GetText string.
        /// </summary>
        /// <param name="entity">
        /// Subject entity to be processed. E.g HTTP response entity or Javascript file.
        /// </param>
        /// <param name="textLocalizer">
        /// Test localization service to use.
        /// May be null when testing this interface. See remarks.
        /// </param>
        /// <param name="languages">
        /// A list of language preferences, sorted in order or preference (most preferred first).
        /// May be null when testing this interface. See remarks.
        /// </param>
        /// <returns>
        /// Processed (and possibly modified) entity.
        /// </returns>
        /// <remarks>
        /// An example replacement is as follows:
        /// <para>
        /// [[[Translate me!]]] -> Übersetzen mich!
        /// </para>
        /// This method supports a testing mode which is enabled by passing httpContext as null.
        /// In this mode, we output "test.message" for every msgid nugget.
        /// </remarks>
        string ProcessNuggets(string entity, ILocalizingService textLocalizer, LanguageItem[] languages);
    }
}
