using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Owin.Pipeline
{
    public class LocalizingMiddleware : OwinMiddleware
    {
        public LocalizingMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public async override Task Invoke(IOwinContext context)
        {
            Console.WriteLine("Begin Request");
            await Next.Invoke(context);
            Console.WriteLine("End Request");
        }
    }
}
