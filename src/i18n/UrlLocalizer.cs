using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n
{
    /// <summary>
    /// The i18n default implementaion of the IUrlLocalizer service.
    /// </summary>
    public class UrlLocalizer : IUrlLocalizer
    {
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
        
        public string ExtractLangTagFromUrl(string url, UriKind uriKind, out string urlPatched)
        {
            return LanguageTag.ExtractLangTagFromUrl(url, uriKind, out urlPatched);
        }
        public string SetLangTagInUrlPath(string url, UriKind uriKind, string langtag)
        {
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
