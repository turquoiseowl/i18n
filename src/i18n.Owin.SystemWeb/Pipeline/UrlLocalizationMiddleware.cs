using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace i18n.Owin.SystemWeb
{
    public class UrlLocalizationMiddleware : OwinMiddleware
    {
        public UrlLocalizationMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public async override Task Invoke(IOwinContext owinContext)
        {
            HttpContextBase context = owinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            Debug.WriteLine("UrlLocalizationMiddleware::Invoke -- ContentType: {0},\n\tUrl: {1}\n\tRawUrl:{2}", context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            // Establish the language for the request. That is, we need to call
            // context.SetPrincipalAppLanguageForRequest with a language, got from the URL,
            // the i18n.langtag cookie, the Accept-Language header, or failing all that the
            // default application language.
            // · If early URL localizer configured, allow it to do it.
            var rootServices = LocalizedApplication.Current.RootServices;
            if (UrlLocalizer.UrlLocalizationScheme != UrlLocalizationScheme.Void
                && rootServices.EarlyUrlLocalizerForApp != null)
            {
                rootServices.EarlyUrlLocalizerForApp.ProcessIncoming(context);
            }
            // · Otherwise skip the URL aspect and detemrine from the other (inferred) attributes.
            else
            {
                context.SetPrincipalAppLanguageForRequest(context.GetInferredLanguage());
            }

            await Next.Invoke(owinContext);
        }
    }
}
