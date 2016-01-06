using System;

namespace i18n
{
    /// <summary>
    /// Enumeration of potential origins of URLs passed through this interface.
    /// </summary>
    public enum UrlOrigin
    {
        /// <summary>
        /// The URL is the URL of a request, specifically one being processed by the 
        /// Early URL Localization logic.
        /// </summary>
        Incoming,

        /// <summary>
        /// The URL is extracted from the response entity and is being offered up for 
        /// localization (patching the response entity).
        /// </summary>
        Outgoing,
    }

    /// <summary>
    /// Describes a service for localizing and un-localizing a URL.
    /// </summary>
    public interface IUrlLocalizer
    {
        /// <summary>
        /// Specifies and controls whether the passed incoming (request) URL should be localized.
        /// </summary>
        /// <param name="url">Subject URL.</param>
        /// <returns>true to localize the URL, false to not localize it.</returns>
        /// <remarks>
        /// This method is called before other methods of this interface during Early URL Localization.
        /// It allows precise control of which URLs to localize over and above any filtering
        /// inherent in the processing (e.g. during outgoing processing, only same-host
        /// URLs are considered for localization and so remote ones will not get this far).
        /// This method is called once per HTTP request.
        /// </remarks>
        bool FilterIncoming(Uri url);

        /// <summary>
        /// Specifies and controls whether a localized form of the passed URL should patched into
        /// the outgoing entity body.
        /// </summary>
        /// <param name="url">Subject URL.</param>
        /// <param name="currentRequestUrl">Url of the current request context. May be null if/when testing.</param>
        /// <returns>true to localize the subject URL, false to not localize it.</returns>
        /// <remarks>
        /// This method is called before other methods of this interface during Late Url Localization.
        /// It allows precise control of which URLs to localize over and above any filtering
        /// inherent in the processing (e.g. during Late processing, only same-host
        /// URLs are considered for localization and so remote ones will not get this far).
        /// This method is typically called many multiple times per HTTP request.
        /// </remarks>
        bool FilterOutgoing(string url, Uri currentRequestUrl);

        /// <summary>
        /// Method for detecting a URL containing a language tag part, and if found outputs
        /// both the language tag and the URL with the that part removed.
        /// </summary>
        /// <param name="url">Either an absolute or relative URL string, as specified by the uriKind parameter.</param>
        /// <param name="uriKind">
        /// Indicates the type of URI in the url parameter. If the URL is known to be relative, this method is more efficient if this 
        /// parameter is set to UriKind.Relative.
        /// </param>
        /// <param name="urlPatched">
        /// On success, set to the URL with the langtag part removed.
        /// On failure, set to value of url param.
        /// </param>
        /// <returns>On success a language tag string instance, otherwise null.</returns>
        /// <remarks>
        /// <para>
        /// The ExtractLangTagFromUrl and InsertLangTagIntoVirtualPath methods
        /// should work symmetrically.
        /// </para>
        /// <para>
        /// In an example implementation, for URL "/zh-Hans/account/signup" we might 
        /// return "zh-Hans" and output "/account/signup".
        /// </para>
        /// </remarks>
        string ExtractLangTagFromUrl(System.Web.HttpContextBase context, string url, UriKind uriKind, bool incomingUrl, out string urlPatched);

        /// <summary>
        /// Patches in the langtag into the passed url path part, replacing any extant langtag 
        /// in the part if necessary.
        /// </summary>
        /// <param name="url">Either an absolute or relative URL string, as specified by the uriKind parameter.</param>
        /// <param name="uriKind">
        /// Indicates the type of URI in the url parameter. If the URL is known to be relative, this method is more efficient if this 
        /// parameter is set to UriKind.Relative.
        /// </param>
        /// <param name="langtag">
        /// Optional langtag to be patched into the part, or null/empty if any langtag 
        /// to be removed from the part.
        /// </param>
        /// <returns>Modified path part string.</returns>
        /// <remarks>
        /// <para>"/account/signup"         , "en" -> "/en/account/signup"</para>
        /// <para>"/zh-Hans/account/signup" , "en" -> "/en/account/signup"</para>
        /// <para>"/zh-Hans/account/signup" , null -> "/account/signup"</para>
        /// </remarks>
        string SetLangTagInUrlPath(System.Web.HttpContextBase context, string url, UriKind uriKind, string langtag);

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
        /// The ExtractLangTagFromUrl and InsertLangTagIntoVirtualPath methods
        /// should work symmetrically.
        /// </para>
        /// <para>
        /// In an example implementation, for langtag "zh-Hans" and virtual path string "account/signup"
        /// we might return "zh-Hans/account/signup".
        /// </para>
        /// </remarks>
        string InsertLangTagIntoVirtualPath(string langtag, string virtualPath);
            //#37 TODO: deprecated.
    }
}
