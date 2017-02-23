using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Adapter.OwinSystemWeb
{
    [Obsolete("This middleware is deprecated and should no longer be used. Refer to the documentation for more information.")]
    public class EntityLocalizationMiddleware : OwinMiddleware
    {
        public EntityLocalizationMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public async override Task Invoke(IOwinContext owinContext)
        {
            System.Web.HttpContextBase context = owinContext.Get<System.Web.HttpContextBase>(typeof(System.Web.HttpContextBase).FullName);
            Debug.WriteLine("OwinMiddleware::Invoke -- ContentType: {0},\n\tUrl: {1}\n\tRawUrl:{2}", context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            LocalizedApplication.InstallResponseFilter(context);

            await Next.Invoke(owinContext);
        }
    }
}
