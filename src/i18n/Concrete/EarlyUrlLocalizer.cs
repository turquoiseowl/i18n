using System;
using System.Web;
using System.Text.RegularExpressions;
using i18n.Helpers;

namespace i18n
{
    public class EarlyUrlLocalizer : IEarlyUrlLocalizer
    {
        private IUrlLocalizer m_urlLocalizer;

        public EarlyUrlLocalizer(IUrlLocalizer urlLocalizer)
        {
            m_urlLocalizer = urlLocalizer;
        }

    #region IEarlyUrlLocalizer

        /// <summary>
        /// Implements the Early Url Localization logic.
        /// <see href="https://docs.google.com/drawings/d/1cH3_PRAFHDz7N41l8Uz7hOIRGpmgaIlJe0fYSIOSZ_Y/edit?usp=sharing"/>
        /// </summary>
        public void ProcessIncoming(
            HttpContextBase context)
        {
            // Is URL explicitly excluded from localization?
            if (!m_urlLocalizer.FilterIncoming(context.Request.Url)) {
                return; } // YES. Continue handling request.

            bool allowRedirect =
                   context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)
                || context.Request.HttpMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase);

            // NO. Is request URL localized?
            string urlNonlocalized;
            string langtag = m_urlLocalizer.ExtractLangTagFromUrl(context, context.Request.RawUrl, UriKind.Relative, true, out urlNonlocalized);
            if (langtag == null)
            {
                // NO.
                // langtag = best match between
                // 1. Inferred user languages (cookie and Accept-Language header)
                // 2. App Languages.
                LanguageTag lt = context.GetInferredLanguage();

                // If redirection allowed...redirect user agent (browser) to localized URL.
                // The principle purpose of this redirection is to ensure the browser is showing the correct URL
                // in its address field.
                if (allowRedirect) {
                    RedirectWithLanguage(context, context.Request.RawUrl, lt.ToString(), m_urlLocalizer);
                    return;
                }
        
                // Otherwise, handle the request under the language infered above but without doing the redirect.
                // NB: this will mean that the user agent (browser) won't have the correct URL displayed;
                // however, this typically won't be an issue because we are talking about POST, PUT and DELETE methods
                // here which are typically not shown to the user.
                else {
                    context.SetPrincipalAppLanguageForRequest(lt);
                    return; // Continue handling request.
                }
            }

            // YES. Does langtag EXACTLY match an App Language?
            LanguageTag appLangTag = LanguageHelpers.GetMatchingAppLanguage(langtag);
            if (appLangTag.IsValid()
                && appLangTag.Equals(langtag))
            {
                // YES. Establish langtag as the PAL for the request.
                context.SetPrincipalAppLanguageForRequest(appLangTag);

                // Rewrite URL for this request.
                context.RewritePath(urlNonlocalized);

                // Continue handling request.
                return;
            }

            // NO. Does langtag LOOSELY match an App Language?
            else if (appLangTag.IsValid()
                && !appLangTag.Equals(langtag))
            {
                // YES. Localize URL with matching App Language.
                // Conditionally redirect user agent to localized URL.
                if (allowRedirect) {
                    RedirectWithLanguage(context, urlNonlocalized, appLangTag.ToString(), m_urlLocalizer);
                    return;
                }
            }
            // NO. Do nothing to URL; expect a 404 which corresponds to language not supported.
            // Continue handling request.
        }

        public string ProcessOutgoing(
            string entity, 
            string langtag, 
            HttpContextBase context)
        {
        // The goal here to localize same-host URLs in the entity body and so save a redirect 
        // on subsequent requests to those URLs by the user-agent (i.e. Early URL Localization
        // of the subsequent request).
        // We patch all URLs in the entity which are:
        //  1. same-host
        //  2. are not already localized
        //  3. are not excluded by custom filtering
        // Examples of attributes containing urls include:
        //   <script src="..."> tags
        //   <img src="..."> tags
        //   <a href="..."> tags
        //   <link href="..."> tags
        // A full list is available here: http://www.w3.org/TR/REC-html40/index/attributes.html
        //

            Uri requestUrl = context != null ? context.Request.Url : null;

            // Localize any HTTP headers in the response containing URLs.
            if (context != null) {
                foreach (string hdr in m_httpHeadersContainingUrls) {
                    string hdrval = context.Response.Headers[hdr];
                    if (!hdrval.IsSet()) {
                        continue; }
                    string urlNew = LocalizeUrl(context, hdrval, langtag, requestUrl, false);
                    if (urlNew == null) {
                        continue; }
                    context.Response.Headers[hdr] = urlNew;
                }
            }
            
            // Localize any nuggets in the entity.
            return m_regexHtmlUrls.Replace(
                entity,
                delegate(Match match)
                {
                    try {
                        string url = match.Groups[2].Value;
                        string urlNew = LocalizeUrl(context, url, langtag, requestUrl, false);
                        // If URL was not changed...leave matched token alone.
                        if (urlNew == null) {
                            return match.Groups[0].Value; } // original

                        // Rebuild and return matched token.
                        string res = string.Format("{0}{1}{2}", 
                            match.Groups[1].Value,
                            urlNew, 
                            match.Groups[3].Value);
                        return res;
                    }
                    catch (System.UriFormatException) {
                        return match.Groups[0].Value; // original
                    }
                });
        }

    #endregion

    // Implementation

        /// <summary>
        /// Regex for finding and replacing urls in html.
        /// </summary>
        public static Regex m_regexHtmlUrls = new Regex(
            "(?<pre><(?:script|img|a|area|link|base|input|frame|iframe|form)\\b.*?(?:src|href|action)\\s*=\\s*[\"']\\s*)(?<url>.+?)(?<post>\\s*[\"'][^>]*?>)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                // The above supports most common ways for a URI to appear in HTML/XHTML.
                // Note that if we fail to catch a URL here, it is not fatal; it only means we don't avoid a redirect
                // round-trip in some cases.
                // TODO: scope for improvement here:
                //  1. Restrict pairing between element and attribute e.g. "script" goes with "src" but not "href".
                //     This will probably require multiple passes with separate regex, one for each attribute name.
                // \s = whitespace
                // See also: http://www.w3.org/TR/REC-html40/index/attributes.html
                // See also: http://www.mikesdotnetting.com/Article/46/CSharp-Regular-Expressions-Cheat-Sheet

        public static string[] m_httpHeadersContainingUrls = new[] { "Location", "Content-Location" };

        protected static void RedirectWithLanguage(
            HttpContextBase context, 
            string urlNonlocalized,
            string langtag,
            IUrlLocalizer m_urlLocalizer)
        {
            // Construct localized URL.
            string urlNew = m_urlLocalizer.SetLangTagInUrlPath(context, urlNonlocalized, UriKind.Relative, langtag);

            // Redirect user agent to new local URL.
            if (LocalizedApplication.Current.PermanentRedirects) {
                context.Response.StatusCode = 301;
                context.Response.Status = "301 Moved Permanently";
            }
            else {
                context.Response.StatusCode = 302;
                context.Response.Status = "302 Moved Temporarily";
            }
            context.Response.RedirectLocation = urlNew;

            // End the request early: no further processing along the pipeline.
            // NB: we did originally use context.Response.End(); here but that causes an
            // unnecessary exception to be thrown (https://support.microsoft.com/en-us/kb/312629) (#195)
            // NB: the line AFTER this line will execute, so best to return immediately here
            // and from caller etc..
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Helper for localizing an individual URL string for a particular langtag value
        /// and URL of the current request.
        /// </summary>
        /// <param name="url">Subject URL to be localized.</param>
        /// <param name="langtag">Language with which to localize the URL.</param>
        /// <param name="requestUrl">URL of the current HTTP request being handled.</param>
        /// <returns>
        /// String describing the new localized URL, or null if the URL was not localized,
        /// either because it was already localized, or because it is from another host, or is explicitly
        /// excluded from localization by the filter.
        /// </returns>
        protected string LocalizeUrl(HttpContextBase context, string url, string langtag, Uri requestUrl, bool incomingUrl)
        {
            // If URL is already localized...leave matched token alone.
            string urlNonlocalized;
            if (m_urlLocalizer.ExtractLangTagFromUrl(context, url, UriKind.RelativeOrAbsolute, incomingUrl, out urlNonlocalized) != null) {
                return null; } // original

            // If URL is not local (i.e. remote host)...leave matched token alone.
            if (requestUrl != null && !requestUrl.IsLocal(url)) {
                return null; } // original

            // Is URL explicitly excluded from localization?
            if (!m_urlLocalizer.FilterOutgoing(url, requestUrl)) {
                return null; } // original

            // Localize the URL.
            return m_urlLocalizer.SetLangTagInUrlPath(context, url, UriKind.RelativeOrAbsolute, langtag);
        }
    }
}
