using System;
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
        /// This method is optimised such that the HttpContextBase instance returned is only created
        /// once per request.
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
        /// Helper for caching a per-request value that identifies the principal language
        /// under which the current request is to be handled.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <param name="pal">Selected AppLanguage.</param>
        public static void SetPrincipalAppLanguageForRequest(this HttpContextBase context, LanguageTag pal)
        {
            context.Items["i18n.PAL"] = pal;
        }
        public static void SetPrincipalAppLanguageForRequest(this HttpContext context, LanguageTag pal)
        {
            context.GetHttpContextBase().SetPrincipalAppLanguageForRequest(pal);
        }

        /// <summary>
        /// Returns any cached per-request value that identifies the principal language
        /// under which the current request is to be handled. That is, the value of any
        /// most-recent call to SetPrincipalAppLanguageForRequest.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>The Principal AppLanguage Language for the reuest, or null if none previously set.</returns>
        public static LanguageTag GetPrincipalAppLanguageForRequest(this HttpContextBase context)
        {
            return (LanguageTag)context.Items["i18n.PAL"];
        }
        public static LanguageTag GetPrincipalAppLanguageForRequest(this HttpContext context)
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
                    = LanguageItem.ParseHttpLanguageHeader(context.Request.Headers["Accept-Language"]);
            }
            return UserLanguages;
        }
        public static LanguageItem[] GetRequestUserLanguages(this HttpContext context)
        {
            return context.GetHttpContextBase().GetRequestUserLanguages();
        }
    }
}
