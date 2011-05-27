using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using i18n.Extensions;

namespace i18n
{
    /// <summary>
    /// A route decorator that adapts existing original routes for localization
    /// </summary>
    internal class LanguageRouteDecorator : RouteDecoratorBase<LanguageRouteDecorator>
    {
        private readonly I18NSession _session;

        public LanguageRouteDecorator(RouteBase route) : base(route)
        {
            _session = new I18NSession();
        }

        public override RouteData GetRouteData(HttpContextBase context)
        {
            var result = _route.GetRouteData(context);

            if(result == null)
            {
                var url = _session.GetUrlFromRequest(context.Request);
                var languages = context.Request.UserLanguages ?? new[] { I18N.DefaultTwoLetterISOLanguageName };
                foreach (var language in languages.Where(language => !string.IsNullOrWhiteSpace(language)))
                {
                    var semiColonIndex = language.IndexOf(';');
                    var token = string.Format("/{0}", semiColonIndex > -1 ? language.Substring(0, semiColonIndex) : language);
                    if (!url.EndsWithAnyIgnoreCase(token, token + "/"))
                    {
                        continue;
                    }

                    url = url.TrimEnd(token.Substring(1).ToCharArray());
                    var originalRequest = new ClonedHttpRequest(context.Request, url);
                    var originalContext = new ClonedHttpContext(context, originalRequest);

                    result = _route.GetRouteData(originalContext);
                    if (result != null)
                    {
                        // Found the original non-decorated route
                        return result;
                    }
                }
            }

            return result;
        }

        public override VirtualPathData GetVirtualPath(RequestContext context, RouteValueDictionary values)
        {
            var result = _route.GetVirtualPath(context, values);

            if (result != null && result.VirtualPath != null)
            {
                var request = context.HttpContext.Request;
                if (!values.ContainsKey("language"))
                {
                    if (request.QueryString["language"] != null)
                    {
                        result.VirtualPath = string.Format("{0}/{1}", result.VirtualPath, request.QueryString["language"]);
                    }
                    else
                    {
                        var language = _session.GetLanguageFromSessionOrService(context.HttpContext);
                        var token = string.Format("/{0}", language);
                        var url = _session.GetUrlFromRequest(context.HttpContext.Request);
                        if(url.EndsWithAnyIgnoreCase(token, string.Format("{0}/", token)))
                        {
                            result.VirtualPath = result.VirtualPath.Equals("")
                                                     ? language
                                                     : string.Format("{0}/{1}", result.VirtualPath, language);
                        }
                    }
                }
                else
                {
                    // Use pre-injected route value
                    var baseUrl = context.HttpContext.Request.Url;
                    if (baseUrl != null)
                    {
                        var relativeUrl = new Uri(baseUrl, values["language"].ToString()).PathAndQuery.Substring(1);
                        result.VirtualPath = relativeUrl;
                    }
                }
            }

            return result;
        }
    }
}
