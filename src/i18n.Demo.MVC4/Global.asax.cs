using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace i18n.Demo.MVC4
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ModelMetadataProviders.Current = new i18n.I18nModelMetadataProvider();
            new i18n.I18N();//constructor must be called, if I18N.Register() is not called, to initalize static properties in I18N class.
            //I18N.Register(), seems to break routing in mvc4 (generates invalid routes), needs further investigation
            //I18N.Register();
        }
    }
}