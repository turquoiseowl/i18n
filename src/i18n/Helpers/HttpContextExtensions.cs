using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using i18n.Helpers;

namespace i18n
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Returns an System.Web.HttpContextBase for the current System.Web.HttpContext.
        /// Facilitates efficient consolidation of methods that require support for both 
        /// System.Web.HttpContext/System.Web.HttpContextBase typed params.
        /// This method is optimised such that the System.Web.HttpContextBase instance returned is only created
        /// once per request.
        /// NB: this may involve a per-appdomain lock when reading from the items dictionary.
        /// </summary>
        public static System.Web.HttpContextBase GetHttpContextBase(this System.Web.HttpContext context)
        {
            // This value is created afresh first time this method is called per request,
            // and cached for the request's remaining calls to this method.
            System.Web.HttpContextBase hcb = context.Items["i18n.System.Web.HttpContextBase"] as System.Web.HttpContextBase;
            if (hcb == null)
            {
                context.Items["i18n.System.Web.HttpContextBase"] 
                    = hcb 
                    = new System.Web.HttpContextWrapper(context);
            }
            return hcb;
        }

        /// <summary>
        /// Returns any translation for the passed individual message.
        /// </summary>
        /// <remarks>
        /// This this the main entry point into i18n library for translating strings.
        /// Selection of acceptable user languages is determined per-request and that
        /// is used to resolve the msgid against any existing localized versions of that string.
        /// Should no translation exist, the msgid string is returned.
        /// </remarks>
        /// <param name="context">Describes the current request.</param>
        /// <param name="msgid">Specifies the individual message to be translated (the first part inside of a nugget). E.g. if the nugget is [[[Sign in]] then this param is "Sign in".</param>
        /// <param name="msgcomment">Specifies the optional message comment value of the subject resource, or null/empty.</param>
        /// <param name="allowLookupWithHtmlDecodedMsgId">
        /// Controls whether a lookup will be attempted with HtmlDecoded-msgid should the first lookup with raw msgid fail.
        /// Defaults to true.
        /// </param>
        /// <returns>Localized string, or msgid if no translation exists.</returns>
        public static string GetText(
            this System.Web.HttpContext context, 
            string msgid, 
            string msgcomment,
            bool allowLookupWithHtmlDecodedMsgId = true)
        {
            return context.GetHttpContextBase().GetText(
                msgid, 
                msgcomment, 
                allowLookupWithHtmlDecodedMsgId);
        }
        public static string GetText(
            this System.Web.HttpContextBase context, 
            string msgid, 
            string msgcomment,
            bool allowLookupWithHtmlDecodedMsgId = true)
        {
           // Relay on to the current text localizer for the appdomain.
            LanguageTag lt;
            return LocalizedApplication.Current.TextLocalizerForApp.GetText(
                allowLookupWithHtmlDecodedMsgId,
                msgid, 
                msgcomment, 
                context.GetRequestUserLanguages(),
                out lt);
        }

        /// <summary>
        /// Returns the translation of the passed string entity which may contain zero or more fully-formed nugget.
        /// </summary>
        /// <param name="context">Describes the current request.</param>
        /// <param name="entity">String containing zero or more fully-formed nuggets which are to be translated according to the language selection of the current request.</param>
        /// <returns>Localized (translated) entity.</returns>
        public static string ParseAndTranslate(this System.Web.HttpContext context, string entity)
        {
            return context.GetHttpContextBase().ParseAndTranslate(entity);
        }
        public static string ParseAndTranslate(this System.Web.HttpContextBase context, string entity)
        {
        // For impl. notes see ResponseFilter.Flush().
        //
           //
            var nuggetLocalizer = LocalizedApplication.Current.NuggetLocalizerForApp;
            if (nuggetLocalizer != null) {
                entity = LocalizedApplication.Current.NuggetLocalizerForApp.ProcessNuggets(
                    entity,
                    context.GetRequestUserLanguages()); }
           //
            if (UrlLocalizer.UrlLocalizationScheme != UrlLocalizationScheme.Void) {
                var earlyUrlLocalizer = LocalizedApplication.Current.EarlyUrlLocalizerForApp;
                if (earlyUrlLocalizer != null) {
                    entity = earlyUrlLocalizer.ProcessOutgoing(
                        entity, 
                        context.GetPrincipalAppLanguageForRequest().ToString(),
                        context); }
            }
           //
            return entity;
        }

        /// <summary>
        /// Helper for caching a per-request value that identifies the principal language
        /// under which the current request is to be handled.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <param name="pal">Selected AppLanguage aka Principle Application Language (PAL).</param>
        /// <param name="updateThreadCulture">
        /// Indicates whether to also update the thread CurrentCulture/CurrentUICulture settings.
        /// </param>
        public static void SetPrincipalAppLanguageForRequest(this System.Web.HttpContext context, ILanguageTag pal, bool updateThreadCulture = true)
        {
            context.GetHttpContextBase().SetPrincipalAppLanguageForRequest(pal, updateThreadCulture);
        }
        public static void SetPrincipalAppLanguageForRequest(this System.Web.HttpContextBase context, ILanguageTag pal, bool updateThreadCulture = true)
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

        /// <summary>
        /// Returns any cached per-request value that identifies the Principle Application Language (PAL)
        /// with which the current request is to be processed. That is, the value of any
        /// most-recent call to SetPrincipalAppLanguageForRequest.
        /// If SetPrincipalAppLanguageForRequest has not yet been called for the request,
        /// returns the default app language.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>
        /// The Principal AppLanguage Language for the request, or the default app language
        /// if none previously set.
        /// </returns>
        public static ILanguageTag GetPrincipalAppLanguageForRequest(this System.Web.HttpContext context)
        {
            return context.GetHttpContextBase().GetPrincipalAppLanguageForRequest();
        }
        public static ILanguageTag GetPrincipalAppLanguageForRequest(this System.Web.HttpContextBase context)
        {
        // The PAL is stored as the first item in the UserLanguages array (with Quality set to 2).
        //
            ILanguageTag langtag = GetRequestUserLanguages(context)[0].LanguageTag;
            if (langtag == null) {
                langtag = LocalizedApplication.Current.DefaultLanguageTag; }
            return langtag;
        }

        /// <summary>
        /// Returns a collection of languages supported by the user-agent, in descending order
        /// of preference. The first item in the collection refers to any Principle Application Language (PAL)
        /// for the request determined by EarlyUrlLocalization (which calls SetPrincipalAppLanguageForRequest),
        /// or is null if EarlyUrlLocalization is disabled.
        /// </summary>
        /// <param name="context">Context of the current request.</param>
        /// <returns>
        /// Array of languages items sorted in order or language preference.
        /// </returns>
        /// <remarks>
        /// This method is optimised such that the collection is built only once per request.
        /// </remarks>
        /// <see>
        /// See LanguageItem.ParseHttpLanguageHeader for more details.
        /// </see>
        public static LanguageItem[] GetRequestUserLanguages(this System.Web.HttpContext context)
        {
            return context.GetHttpContextBase().GetRequestUserLanguages();
        }
        public static LanguageItem[] GetRequestUserLanguages(this System.Web.HttpContextBase context)
        {
            // Determine UserLanguages.
            // This value is created afresh first time this method is called per request,
            // and cached for the request's remaining calls to this method.
            LanguageItem[] UserLanguages = context.Items["i18n.UserLanguages"] as LanguageItem[];
            if (UserLanguages == null)
            {
                // Construct UserLanguages list and cache it for the rest of the request.
	            context.Items["i18n.UserLanguages"] = UserLanguages = GetRequestUserLanguagesImplementation(context);
            }
            return UserLanguages;
        }

		/// <summary>
		/// Describes a procedure for determining the user languages for the current request.
		/// </summary>
		/// <param name="context">
		/// Describes the current request. May be null if called outside of any request.
		/// </param>
		/// <returns>The language items which are determined for the current current request.</returns>
		/// <remarks>
		/// <see cref="HttpContextExtensions.GetRequestUserLanguagesImplementation"/>
		/// </remarks>
		public delegate LanguageItem[] GetRequestUserLanguagesProc(System.Web.HttpContextBase context);

		/// <summary>
		/// Registers the procedure used by instances of this class for determining the 
		/// available user languages for the current request.
		/// </summary>
		/// <remarks>
		/// The default implementation will check the `Accept-Language` header attribute 
		/// for the available languages in the current request.
		/// </remarks>
		public static GetRequestUserLanguagesProc GetRequestUserLanguagesImplementation { get; set; } = (context) =>
		{
			return LanguageItem.ParseHttpLanguageHeader(context.Request.Headers["Accept-Language"]);
			// NB: originally we passed LocalizedApplication.Current.DefaultLanguageTag
			// here as the second parameter i.e. to specify the PAL. However, this was
			// found to be incorrect when operating i18n with EarlyUrlLocalization disabled,
			// as SetPrincipalAppLanguageForRequest was not being called, that normally
			// overwriting the PAL set erroneously here.
		};

		/// <summary>
		/// Add a Content-Language HTTP header to the response, based on any languages
		/// that have provided resources during the request.
		/// </summary>
		/// <param name="context">Context of the current request.</param>
		/// <returns>
		/// true if header added; false if no languages provided content during the request and
		/// so no header was added.
		/// </returns>
		public static bool SetContentLanguageHeader(this System.Web.HttpContext context)
        {
            return context.GetHttpContextBase().SetContentLanguageHeader();
        }
        public static bool SetContentLanguageHeader(this System.Web.HttpContextBase context)
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

        /// <summary>
        /// Returns the language for the current request inferred from the request context:
        /// that is, attributes of the request other that the URL.
        /// </summary>
        /// <remarks>
        /// The language is infered from the following attributes of the request,
        /// in order of preference:<br/>
        ///     i18n.langtag cookie<br/>
        ///     Accept-Language header<br/>
        ///     fall back to i18n.LocalizedApplication.Current.DefaultLanguage<br/>
        /// Additionally, each language is matched by the language matching algorithm
        /// against the set of application languages available.
        /// </remarks>
        /// <param name="context">Context of the current request.</param>
        /// <returns>
        /// Returns language tag describing the inferred language.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Expected GetRequestUserLanguages to fall back to default language.
        /// </exception>
        public static LanguageTag GetInferredLanguage(this System.Web.HttpContext context)
        {
            return context.GetHttpContextBase().GetInferredLanguage();
        }
        public static LanguageTag GetInferredLanguage(this System.Web.HttpContextBase context)
        {
            // langtag = best match between
            // 1. Inferred user languages (cookie and Accept-Language header)
            // 2. App Languages.
            LanguageTag lt = null;
            System.Web.HttpCookie cookie_langtag = context.Request.Cookies.Get("i18n.langtag");
            if (cookie_langtag != null) {
                lt = LanguageHelpers.GetMatchingAppLanguage(cookie_langtag.Value); }
            if (lt == null) {
                lt = LanguageHelpers.GetMatchingAppLanguage(context.GetRequestUserLanguages()); }
            if (lt == null) {
                throw new InvalidOperationException("Expected GetRequestUserLanguages to fall back to default language."); }
            return lt;
        }

        /// <summary>
        /// Runs the Language Matching Algorithm for the UserLanguages of the current request against
        /// the specified array of AppLanguages, returning the AppLanguage determined to be the best match.
        /// </summary>
        /// <param name="context">Context of the current request.</param>
        /// <param name="AppLanguages">
        /// The list of languages in which an arbitrary resource is available.
        /// </param>
        /// <returns>
        /// LanguageTag instance selected from AppLanguages with the best match, or null if there is no match
        /// at all (or UserLanguages and/or AppLanguages is empty).
        /// It is possible for there to be no match at all if no language subtag in the UserLanguages tags
        /// matches the same of any of the tags in AppLanguages list.
        /// </returns>
        public static LanguageTag ChooseAppLanguage(this System.Web.HttpContext context, IEnumerable<LanguageTag> AppLanguages)
        {
            return context.GetHttpContextBase().ChooseAppLanguage(AppLanguages);
        }
        public static LanguageTag ChooseAppLanguage(this System.Web.HttpContextBase context, IEnumerable<LanguageTag> AppLanguages)
        {
            string text;
            return LanguageMatching.MatchLists(
                context.GetRequestUserLanguages(),
                AppLanguages, 
                null, 
                null, 
                out text);
        }
    }
}
