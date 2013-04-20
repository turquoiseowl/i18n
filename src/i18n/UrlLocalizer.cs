using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n
{
    /// <summary>
    /// Enumerate various approaches to handling and redirection of localized URLs.
    /// </summary>
    public enum UrlLocalizationScheme
    {
        /// <summary>
        /// Everything is explicit, so any URLs/routes not containing a language tag are patched 
        /// and redirected, whether or not the language is the app-default.
        /// </summary>
        /// <remarks>
        /// E.g. if selected language for the request is en then
        /// <para>
        /// example.com -> example.com/fr
        /// example.com/fr -> example.com/fr
        /// </para>
        /// </remarks>
        Scheme1,

        /// <summary>
        /// Everything to be explicit except the default language which MAY be implicit.
        /// </summary>
        /// <remarks>
        /// E.g. if selected language for the request is en then
        /// <para>
        /// example.com -> example.com/fr
        /// example.com/fr -> example.com/fr
        /// </para>
        /// </remarks>
        Scheme2,

        /// <summary>
        /// Everything to be explicit except the default language which MUST be implicit.
        /// </summary>
        /// <remarks>
        /// E.g. if selected language for the request is en then
        /// <para>
        /// example.com -> example.com
        /// example.com/fr -> example.com
        /// </para>
        /// </remarks>
        Scheme3,
    }

    /// <summary>
    /// The i18n default implementaion of the IUrlLocalizer service.
    /// </summary>
    public class UrlLocalizer : IUrlLocalizer
    {

    // Implementation

        /// <summary>
        /// Specifies the URL localization used by ALL instances of UrlLocalizer.
        /// May be changed in application start.
        /// </summary>
        /// <remarks>
        /// Presently, only Scheme1 and Scheme2 are supported by this class.
        /// </remarks>
        public static UrlLocalizationScheme UrlLocalizationScheme = UrlLocalizationScheme.Scheme1;

        /// <summary>
        /// Describes one or more procedures for filtering a URL during Early URL Localization.
        /// </summary>
        /// <param name="url">Subject URL.</param>
        /// <returns>true if URL is to be localized, false if not.</returns>
        public delegate bool IncomingUrlFilter(Uri url);

        /// <summary>
        /// Describes one or more procedures for filtering a URL during Late URL Localization.
        /// </summary>
        /// <param name="url">Subject URL.</param>
        /// <param name="currentRequestUrl">Url of the current request context. May be null if/when testing.</param>
        /// <returns>true if URL is to be localized, false if not.</returns>
        public delegate bool OutgoingUrlFilter(string url, Uri currentRequestUrl);

        /// <summary>
        /// Filters that examines the request URL during Early URL Localization
        /// and returns an indication as to whether the URL should be localized.
        /// </summary>
        /// <remarks>
        /// In the case of multiple filters added to this member, they all need
        /// to return true for the URL to be localized.
        /// </remarks>
        public static IncomingUrlFilter IncomingUrlFilters { get; set; }

        /// <summary>
        /// Filters that examines the request URL during Late URL Localization
        /// and returns an indication as to whether the URL should be localized.
        /// </summary>
        /// <remarks>
        /// In the case of multiple filters added to this member, they all need
        /// to return true for the URL to be localized.
        /// </remarks>
        public static OutgoingUrlFilter OutgoingUrlFilters { get; set; }

    #region [IUrlLocalizer]

        public bool FilterIncoming(Uri url)
        {
            // Run through any filters installed.
            if (IncomingUrlFilters != null) {
                foreach (IncomingUrlFilter filter in IncomingUrlFilters.GetInvocationList())
                {
                    if (!filter(url)) {
                        return false; }
                }
            }
            return true;
        }
        public bool FilterOutgoing(string url, Uri currentRequestUrl)
        {
            // Run through any filters installed.
            if (OutgoingUrlFilters != null) {
                foreach (OutgoingUrlFilter filter in OutgoingUrlFilters.GetInvocationList())
                {
                    if (!filter(url, currentRequestUrl)) {
                        return false; }
                }
            }
            return true;
        }
        
        public string ExtractLangTagFromUrl(string url, UriKind uriKind, bool incomingUrl, out string urlPatched)
        {
            string result = LanguageTag.ExtractLangTagFromUrl(url, uriKind, out urlPatched);

            switch (UrlLocalizationScheme)
            {
                case UrlLocalizationScheme.Scheme1:
                {
                    return result;
                }
                case UrlLocalizationScheme.Scheme2:
                {
                    // If the URL is nonlocalized incoming URL, this implies default language.
                    if (result == null && incomingUrl) {
                        result = LocalizedApplication.Current.DefaultLanguage;
                        urlPatched = url;
                    }
                    return result;
                }
                case UrlLocalizationScheme.Scheme3:
                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public string SetLangTagInUrlPath(string url, UriKind uriKind, string langtag)
        {
            switch (UrlLocalizationScheme)
            {
                case UrlLocalizationScheme.Scheme2:
                {
                    if (LocalizedApplication.Current.DefaultLanguageTag.Equals(langtag)) {
                        return url; }
                    break;
                }
                case UrlLocalizationScheme.Scheme3:
                {
                    throw new InvalidOperationException();
                }
            }

            return LanguageTag.SetLangTagInUrlPath(url, uriKind, langtag);
        }
        public string InsertLangTagIntoVirtualPath(string langtag, string virtualPath)
        {
            // Prepend the virtual path with the PAL langtag.
            // E.g. "account/signup" -> "fr-CH/account/signup"
            // E.g. ""               -> "fr-CH"
            return virtualPath.IsSet() ? string.Format("{0}/{1}", langtag, virtualPath) : langtag;
        }

    #endregion

    }
}
