using System;
using System.Linq;
using System.Web;
using System.Web.Routing;

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
                    string suffix = url.EndsWithAnyIgnoreCase(token, token + "/");
                    if (suffix == null)
                    {
                        continue;
                    }

                    url = url.Substring(0, url.Length -suffix.Length);
                        //BUGBUG: the above can result in a zero-length URL, which in turn
                        // cause the ClonedHttpRequest.AppRelativeCurrentExecutionFilePath method
                        // to throw ArgumentNull exception when called indirectly.
                        // Fix is simple i.e. 
/*
                    if (url.Length == 0) {
                        url = "/"; }
*/
                            // However, reluctant to alter this code as it is not well documented and
                            // don't want to break something else.

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
                        if (url.EndsWithAnyIgnoreCase(token, string.Format("{0}/", token)) != null)
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
                            //BUGBUG: the above is erroneous as the Uri constructor does not simply append: it
                            // assumes the first, baseUri argument has no local path component which is not
                            // a safe assumption. E.g. /a/b + c = /a/c. I.e. the action get dropped.
                            // A fix would probebly go something like this:
/*
                        // Append language to relative url.
                        // E.g. account/signup?xyz -> account/signup/de?xyz
                        // NB: originally the Uri class was used here to combine the paths. This was a bug
                        // as the Uri constructor baseUri param assumes a Uri without a path, which is not
                        // a safe assumption here.
                        string relativeUrl = baseUrl.LocalPath;
                        if (!relativeUrl.EndsWith("/")) {
                            relativeUrl += "/"; }
                        relativeUrl += values["language"] + baseUrl.Query;
                        relativeUrl = relativeUrl.Substring(1);
 */
                            // However, reluctant to alter this code as it is not well documented and
                            // don't want to break something else.

                        result.VirtualPath = relativeUrl;
                    }
                }
            }

            return result;
        }
    }
}
