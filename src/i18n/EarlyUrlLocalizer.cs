using System;
using System.Web;
using System.Text.RegularExpressions;

namespace i18n
{
    public class EarlyUrlLocalizer : IEarlyUrlLocalizer
    {

    #region IEarlyUrlLocalizer

        /// <summary>
        /// Implements the Early Url Localization logic.
        /// <see href="https://docs.google.com/drawings/d/1cH3_PRAFHDz7N41l8Uz7hOIRGpmgaIlJe0fYSIOSZ_Y/edit?usp=sharing"/>
        /// </summary>
        public void ProcessIncoming(
            HttpContextBase context,
            IUrlLocalizer urlLocalizer)
        {
            // Is URL explicitly excluded from localization?
            if (!urlLocalizer.FilterIncoming(context.Request.Url)) {
                return; } // YES. Continue handling request.

            // NO. Is request URL localized?
            string urlNonlocalized;
            string langtag = urlLocalizer.ExtractLangTagFromUrl(context.Request.RawUrl, UriKind.Relative, true, out urlNonlocalized);
            if (langtag == null)
            {
                // NO.
                // langtag = best match between
                // 1. Inferred user languages (cookie and Accept-Language header)
                // 2. App Languages.
                LanguageTag lt = null;
                HttpCookie cookie_langtag = context.Request.Cookies.Get("i18n.langtag");
                if (cookie_langtag != null) {
                    lt = LanguageHelpers.GetMatchingAppLanguage(cookie_langtag.Value); }
                if (lt == null) {
                    lt = LanguageHelpers.GetMatchingAppLanguage(context.GetRequestUserLanguages()); }
                if (lt == null) {
                    throw new InvalidOperationException("Expected GetRequestUserLanguages to fall back to default language."); }

                // Redirect user agent to localized URL.
                RedirectWithLanguage(context, lt.ToString(), urlLocalizer);
                return;
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
                // Redirect user agent to localized URL.
                RedirectWithLanguage(context, appLangTag.ToString(), urlLocalizer);
                return;
            }
            // NO. Do nothing to URL; expect a 404 which corresponds to language not supported.
            // Continue handling request.
        }

        public string ProcessOutgoing(
            string entity, 
            string langtag, 
            HttpContextBase context,
            IUrlLocalizer urlLocalizer)
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
            return m_regexHtmlUrls.Replace(
                entity,
                delegate(Match match)
                {
                    try {
                        string url = match.Groups[2].Value;
                        
                        // If URL is already localized...leave matched token alone.
                        string urlNonlocalized;
                        if (urlLocalizer.ExtractLangTagFromUrl(url, UriKind.RelativeOrAbsolute, false, out urlNonlocalized) != null) {
                            return match.Groups[0].Value; } // original

                        // If URL is not local (i.e. remote host)...leave matched token alone.
                        if (requestUrl != null && !requestUrl.IsLocal(url)) {
                            return match.Groups[0].Value; } // original

                        // Is URL explicitly excluded from localization?
                        if (!LocalizedApplication.UrlLocalizer.FilterOutgoing(url, requestUrl)) {
                            return match.Groups[0].Value; } // original

                        // Localized the URL.
                        url = urlLocalizer.SetLangTagInUrlPath(url, UriKind.RelativeOrAbsolute, langtag);

                        // Rebuild and return matched token.
                        string res = string.Format("{0}{1}{2}", 
                            match.Groups[1].Value,
                            url, 
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
                // Note that if we fail to catch a URL here, it is not fata; only means we don't avoid a redirect
                // round-trip in some cases.
                // TODO: scope for improvement here:
                //  1. Restrict pairing between element and attribute e.g. "script" goes with "src" but not "href".
                //     This will probably require multiple passes with separate regex, one for each attribute name.
                // \s = whitespace
                // See also: http://www.w3.org/TR/REC-html40/index/attributes.html
                // See also: http://www.mikesdotnetting.com/Article/46/CSharp-Regular-Expressions-Cheat-Sheet

        protected static void RedirectWithLanguage(
            HttpContextBase context, 
            string langtag,
            IUrlLocalizer urlLocalizer)
        {
            // Construct localized URL.
            string urlNew = urlLocalizer.SetLangTagInUrlPath(context.Request.RawUrl, UriKind.Relative, langtag);

            // Redirect user agent to new local URL.
            if (LocalizedApplication.PermanentRedirects) {
                context.Response.StatusCode = 301;
                context.Response.Status = "301 Moved Permanently";
            }
            else {
                context.Response.StatusCode = 302;
                context.Response.Status = "302 Moved Temporarily";
            }
            context.Response.RedirectLocation = urlNew;
            context.Response.End();
        }
    }
}
