using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using i18n.Extensions;

namespace i18n
{
    /// <summary>
    /// A global filter for automatically redirecting localized requests
    /// to appropriate URLs, as well as setting the resulting language in
    /// the HTTP response
    /// </summary>
    public class LanguageFilter : IActionFilter 
    {
        private const string ContentLanguageHeader = "Content-Language";
        private readonly I18NSession _session;
        private readonly ILocalizingService _service;

        public LanguageFilter()
        {
            _session = new I18NSession();
            _service = DependencyResolver.LocalizingService;
        }

        /// <summary>
        /// Called before an action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var values = filterContext.RouteData.Values;

            // Value is already injected from a route declaration
            if (values.ContainsKey("language"))
            {
                return;
            }

            // Value is part of the explicit route and is the preferred language (from those available)
            var preferred = _session.GetLanguageFromSessionOrService(filterContext.HttpContext);
            var url = _session.GetUrlFromRequest(filterContext.HttpContext.Request);
            if (url.EndsWithAnyIgnoreCase(string.Format("/{0}", preferred), string.Format("/{0}/", preferred)))
            {
                return;
            }

            // Value is loose in the query string, i.e. '/?language=en'
            if (request.QueryString["language"] != null)
            {
                _session.Set(filterContext.HttpContext, request.QueryString["language"]);
                preferred = _session.GetLanguageFromSessionOrService(filterContext.HttpContext);
            }

            // Value is part of the explicit route, i.e. '/about/en' but not the preferred language
            var languages = request.UserLanguages ?? new[] { I18N.DefaultTwoLetterISOLanguageName };
            foreach (var language in languages.Where(language => !string.IsNullOrWhiteSpace(language)))
            {
                var semiColonIndex = language.IndexOf(';');
                var token = string.Format("/{0}", semiColonIndex > -1 ? language.Substring(0, semiColonIndex) : language);
                if (!url.EndsWithAnyIgnoreCase(token, string.Format("{0}/", token)))
                {
                    continue;
                }

                // This is an explicit language request, override preferences
                _session.Set(filterContext.HttpContext, token.Substring(1));
                preferred = _session.GetLanguageFromSessionOrService(filterContext.HttpContext);
            }
            
            RedirectWithLanguage(filterContext, values, preferred);
        }
        
        private static void RedirectWithLanguage(ControllerContext filterContext, RouteValueDictionary values, string language)
        {
            if(!values.ContainsKey("language"))
            {
                values.Add("language", language);
            }

            var helper = new UrlHelper(filterContext.RequestContext);
            var url = helper.RouteUrl(values);

            var result = new RedirectResult(url);
            result.ExecuteResult(filterContext);
        }

        /// <summary>
        /// Called after the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var language = I18NSession.GetLanguageFromSession(filterContext.HttpContext)
                           ?? _service.GetBestAvailableLanguageFrom(filterContext.HttpContext.Request.UserLanguages)
                           ?? I18N.DefaultTwoLetterISOLanguageName;

            filterContext.HttpContext.Response.AppendHeader(ContentLanguageHeader, language);
        }
    }
}
