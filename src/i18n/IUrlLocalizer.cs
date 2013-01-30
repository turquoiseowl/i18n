using System.Web;

namespace i18n
{
    /// <summary>
    /// Describes a service for localizing and un-localizing a URL.
    /// </summary>
    public interface IUrlLocalizer
    {
        /// <summary>
        /// Method for detecting a URL containing a language tag part, and if found outputs
        /// both the language tag and the URL with the that part removed.
        /// </summary>
        /// <param name="url">
        /// URL to be inspected for a valid langtag part.
        /// </param>
        /// <param name="urlPatched">
        /// On success, set to the URL with the langtag part removed.
        /// On failure, set to value of url param.
        /// </param>
        /// <returns>On success a language tag string instance, otherwise null.</returns>
        /// <remarks>
        /// <para>
        /// In an example implementation, for URL "/zh-Hans/account/signup" we might 
        /// return "zh-Hans" and output "/account/signup".
        /// </para>
        /// </remarks>
        string ExtractLangTagFromUrl(string url, out string urlPatched);


        /// <summary>
        /// Method for injecting a language tag into a route's virtual path.
        /// </summary>
        /// <param name="langtag">Subject language tag.</param>
        /// <param name="virtualPath">
        /// Virtual path to be patched. E.g. "account/signup".
        /// Note that virtual paths do not begin with a forward-slash, and root path
        /// is an empty string.
        /// </param>
        /// <returns>
        /// Patched virtual path string.
        /// </returns>
        /// <remarks>
        /// <para>
        /// In an example implementation, for langtag "zh-Hans" and virtual path string "account/signup"
        /// we might return "zh-Hans/account/signup".
        /// </para>
        /// </remarks>
        string InsertLangTagIntoVirtualPath(string langtag, string virtualPath);
    }


}
