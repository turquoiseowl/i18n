using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace i18n
{
/*
    LocalizingModule can be installed like this:

        IIS7+ Integrated mode:

          <system.webServer>
            <modules>
              <add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" />
            </modules>
          </system.webServer>

        IIS7 Classic mode and II6:

          <system.web>
            <httpModules>
              <add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" /> <!-- #37 -->
            </httpModules>
          </system.web>
*/

    /// <summary>
    /// HTTP module responsible for:
    /// 1. Implementing early URL localization
    /// 2. Installing our ResponseFilter into the ASP.NET pipeline.
    /// </summary>
    public class LocalizingModule : IHttpModule
    {

    #region [IHttpModule]

        public void Init(HttpApplication application)
        {
            DebugHelpers.WriteLine("LocalizingModule::Init -- application: {0}", application);
            
            // Wire up our event handlers into the ASP.NET pipeline.
            if (LocalizedApplication.EnableEarlyUrlLocalization) {
                application.BeginRequest += OnBeginRequest; }
            application.ReleaseRequestState += OnReleaseRequestState;
        }
        public void Dispose() {}

    #endregion

    // Implementation

        protected static void RedirectWithLanguage(HttpContextBase context, string langtag)
        {
            // Construct localized URL.
            string urlNew = LocalizedApplication.UrlLocalizer.SetLangTagInUrlPath(context.Request.RawUrl, UriKind.Relative, langtag);

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

        /// <summary>
        /// Implements the Early Url Localization logic.
        /// <see href="https://docs.google.com/drawings/d/1cH3_PRAFHDz7N41l8Uz7hOIRGpmgaIlJe0fYSIOSZ_Y/edit?usp=sharing"/>
        /// </summary>
        protected static void EarlyUrlLocalization(HttpContextBase context)
        {
            // Is URL explicitly excluded from localization?
            if (!LocalizedApplication.UrlLocalizer.FilterIncoming(context.Request.Url)) {
                return; } // YES. Continue handling request.

            // NO. Is request URL localized?
            string urlNonlocalized;
            string langtag = LocalizedApplication.UrlLocalizer.ExtractLangTagFromUrl(context.Request.RawUrl, UriKind.Relative, out urlNonlocalized);
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
                RedirectWithLanguage(context, lt.ToString());
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
                RedirectWithLanguage(context, appLangTag.ToString());
                return;
            }
            // NO. Do nothing to URL; expect a 404 which corresponds to language not supported.
            // Continue handling request.
        }

    // Events handlers

        /// <summary>
        /// Handler for the BeginRequest ASP.NET request pipeline event, where we inject our
        /// Early URL Localization logic.
        /// </summary>
        private void OnBeginRequest(object sender, EventArgs e)
        {
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("LocalizingModule::OnBeginRequest -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            if (LocalizedApplication.EnableEarlyUrlLocalization) {
                EarlyUrlLocalization(context); }
        }

        /// <summary>
        /// Handler for the ReleaseRequestState ASP.NET request pipeline event.
        /// This event occurs late on in the pipeline but prior to the response being filtered.
        /// We take the opportunity to inject our i8n post-processing of the response.
        /// </summary>
        private void OnReleaseRequestState(object sender, EventArgs e)
        {
        //
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("LocalizingModule::OnReleaseRequestState -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            // If the content type of the entity is eligible for processing...wire up our filter
            // to do the processing. The entity data will be run through the filter a bit later on
            // in the pipeline.
            if (LocalizedApplication.ContentTypesToLocalize != null
                && LocalizedApplication.ContentTypesToLocalize.Match(context.Response.ContentType).Success) {
                DebugHelpers.WriteLine("LocalizingModule::OnReleaseRequestState -- Installing filter");
                context.Response.Filter = new ResponseFilter(context, context.Response.Filter);
            }
        }
    }
}
