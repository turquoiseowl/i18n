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
    public class EntityLocalizationMiddleware : OwinMiddleware
    {
        public EntityLocalizationMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public async override Task Invoke(IOwinContext owinContext)
        {
            HttpContextBase context = owinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            Debug.WriteLine("OwinMiddleware::Invoke -- ContentType: {0},\n\tUrl: {1}\n\tRawUrl:{2}", context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            // If the content type of the entity is eligible for processing AND the URL is not to be excluded,
            // wire up our filter to do the processing. The entity data will be run through the filter a
            // bit later on in the pipeline.
            if ((LocalizedApplication.Current.ContentTypesToLocalize != null
                    && LocalizedApplication.Current.ContentTypesToLocalize.Match(context.Response.ContentType).Success) // Include certain content types from being processed
                    )
            {
                if ((LocalizedApplication.Current.UrlsToExcludeFromProcessing != null
                    && LocalizedApplication.Current.UrlsToExcludeFromProcessing.Match(context.Request.RawUrl).Success) // Exclude certain URLs from being processed
                    )
                {
                    Debug.WriteLine("LocalizingModule::OnReleaseRequestState -- Bypassing filter, URL excluded: ({0}).", context.Request.RawUrl);
                }
                else if ((context.Response.Headers["Content-Encoding"] != null
                    || context.Response.Headers["Content-Encoding"] == "gzip") // Exclude responses that have already been compressed earlier in the pipeline
                )
                {
                    Debug.WriteLine("LocalizingModule::OnReleaseRequestState -- Bypassing filter, response compressed.");
                }
                else
                {
                    var rootServices = LocalizedApplication.Current.RootServices;
                    Debug.WriteLine("LocalizingModule::OnReleaseRequestState -- Installing filter");
                    context.Response.Filter = new ResponseFilter(
                        context,
                        context.Response.Filter,
                        UrlLocalizer.UrlLocalizationScheme == UrlLocalizationScheme.Void ? null : rootServices.EarlyUrlLocalizerForApp,
                        rootServices.NuggetLocalizerForApp);
                }
            }
            else
            {
                Debug.WriteLine("LocalizingModule::OnReleaseRequestState -- Bypassing filter, No content-type match: ({0}).", context.Response.ContentType);
            }

            await Next.Invoke(owinContext);
        }
    }
}
