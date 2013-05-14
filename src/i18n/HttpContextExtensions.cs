using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace i18n
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Returns an HttpContextBase for the current HttpContext.
        /// Facilitates efficient consolidation of methods that require support for both 
        /// HttpContext/HttpContextBase typed params.
        /// This method is optimised such that the HttpContextBase instance returned is only created
        /// once per request.
        /// NB: this may involve a per-appdomain lock when reading from the items dictionary.
        /// </summary>
        public static HttpContextBase GetHttpContextBase(this HttpContext context)
        {
            // This value is created afresh first time this method is called per request,
            // and cached for the request's remaining calls to this method.
            HttpContextBase hcb = context.Items["i18n.HttpContextBase"] as HttpContextBase;
            if (hcb == null)
            {
                context.Items["i18n.HttpContextBase"] 
                    = hcb 
                    = new HttpContextWrapper(context);
            }
            return hcb;
        }

        /// <summary>
        /// Returns any translation for the passed message.
        /// </summary>
        /// <remarks>
        /// This this the main entry point into i18n library for translating strings.
        /// Selection of acceptable user languages is determined per-request and that
        /// is used to resolve the msgid against any existing localized versions of that string.
        /// Should no translation exist, the msgid string is returned.
        /// </remarks>
        /// <param name="context">Describes the current request.</param>
        /// <param name="msgid">Identifies the message to be translated.</param>
        /// <returns>Localized string, or msgid if no translation exists.</returns>
        public static string GetText(this HttpContextBase context, string msgid)
        {
            // Lookup resource.
            LanguageTag lt;
            msgid = LocalizedApplication.Current.TextLocalizerForApp.GetText(msgid, context.GetRequestUserLanguages(), out lt) ?? msgid;
            return HttpUtility.HtmlDecode(msgid);
        }
        public static string GetText(this HttpContext context, string msgid)
        {
            return context.GetHttpContextBase().GetText(msgid);
        }

        /// <summary>
        /// Helper for caching a per-request value that identifies the principal language
        /// under which the current request is to be handled.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <param name="pal">Selected AppLanguage.</param>
        /// <param name="updateThreadCulture">
        /// Indicates whether to also update the thread CurrentCulture/CurrentUICulture settings.
        /// </param>
        public static void SetPrincipalAppLanguageForRequest(this HttpContextBase context, ILanguageTag pal, bool updateThreadCulture = true)
        {
        // The PAL is stored as the first item in the UserLanguages array (with Quality set to 2).
        //
            LanguageItem[] UserLanguages = GetRequestUserLanguages(context);
            UserLanguages[0] = new LanguageItem(pal, LanguageItem.PalQualitySetting, 0);

            // Run through any handlers installed for this event.
            if (LocalizedApplication.Current.SetPrincipalAppLanguageForRequestHandlers != null) {
                foreach (LocalizedApplication.SetLanguageHandler handler in LocalizedApplication.Current.SetPrincipalAppLanguageForRequestHandlers.GetInvocationList())
                {
                    handler(context, pal);
                }
            }

        }
        public static void SetPrincipalAppLanguageForRequest(this HttpContext context, ILanguageTag pal)
        {
            context.GetHttpContextBase().SetPrincipalAppLanguageForRequest(pal);
        }

        /// <summary>
        /// Returns any cached per-request value that identifies the principal language
        /// under which the current request is to be handled. That is, the value of any
        /// most-recent call to SetPrincipalAppLanguageForRequest.
        /// If SetPrincipalAppLanguageForRequest has not yet been called for the request,
        /// returns the default app language.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>
        /// The Principal AppLanguage Language for the request, or the default app language
        /// if none previously set.
        /// </returns>
        public static ILanguageTag GetPrincipalAppLanguageForRequest(this HttpContextBase context)
        {
        // The PAL is stored as the first item in the UserLanguages array (with Quality set to 2).
        //
            ILanguageTag langtag = GetRequestUserLanguages(context)[0].LanguageTag;
            if (langtag == null) {
                langtag = LocalizedApplication.Current.DefaultLanguageTag; }
            return langtag;
        }
        public static ILanguageTag GetPrincipalAppLanguageForRequest(this HttpContext context)
        {
            return context.GetHttpContextBase().GetPrincipalAppLanguageForRequest();
        }

        /// <summary>
        /// Returns an ordered collection of languages supported by the user-agent.
        /// See LanguageItem.ParseHttpLanguageHeader for more details.
        /// This method is optimised such that the collection is built only once per request.
        /// </summary>
        /// <param name="context">Context of the current request.</param>
        /// <returns>
        /// Array of languages items sorted in order or language preference.
        /// </returns>
        public static LanguageItem[] GetRequestUserLanguages(this HttpContextBase context)
        {
            // Determine UserLanguages.
            // This value is created afresh first time this method is called per request,
            // and cached for the request's remaining calls to this method.
            LanguageItem[] UserLanguages = context.Items["i18n.UserLanguages"] as LanguageItem[];
            if (UserLanguages == null)
            {
                // Construct UserLanguages list and cache it for the rest of the request.
                context.Items["i18n.UserLanguages"] 
                    = UserLanguages 
                    = LanguageItem.ParseHttpLanguageHeader(
                        context.Request.Headers["Accept-Language"],
                        LocalizedApplication.Current.DefaultLanguageTag);
            }
            return UserLanguages;
        }
        public static LanguageItem[] GetRequestUserLanguages(this HttpContext context)
        {
            return context.GetHttpContextBase().GetRequestUserLanguages();
        }

        /// <summary>
        /// Add a Content-Language HTTP header to the response, based on any languages
        /// that have provided resources during the request.
        /// </summary>
        /// <param name="context">Context of the current request.</param>
        /// <returns>
        /// true if header added; false if no languages provided content during the request and
        /// so no header was added.
        /// </returns>
        public static bool SetContentLanguageHeader(this HttpContextBase context)
        {
           // Enumerate the possible user languages for the request. For any that have provided
           // a resource, add them to the header value.
            StringBuilder sb = new StringBuilder();
            LanguageItem[] langitems = context.GetRequestUserLanguages();
            foreach (LanguageItem langUser in langitems)
            {
                if (langUser.LanguageTag == null) {
                    continue; }
                if (langUser.UseCount > 0) {
                    if (sb.Length > 0) {
                        sb.Append(","); }
                    sb.Append(langUser.LanguageTag.ToString());
                }
            }
            if (sb.Length == 0) {
                return false; }
            context.Response.AppendHeader("Content-Language", sb.ToString());
            return true;
        }
        public static bool SetContentLanguageHeader(this HttpContext context)
        {
            return context.GetHttpContextBase().SetContentLanguageHeader();
        }

    }
}
