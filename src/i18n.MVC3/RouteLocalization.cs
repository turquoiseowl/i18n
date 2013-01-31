using container;

namespace i18n
{
    /// <summary>
    /// Describes the configuration of the i18n MVC Route Localization feature.
    /// </summary>
    public static class RouteLocalization
    {
        internal static Container Container { get; set; }
        
        static RouteLocalization()
        {
            Container = new Container();
            Container.Register<IUrlLocalizer>(r => new UrlLocalizer());
        }

        public static IUrlLocalizer UrlLocalizer
        {
            get { return Container.Resolve<IUrlLocalizer>(); }
            set
            {
                Container.Remove<IUrlLocalizer>();
                Container.Register(r => value);
            }
        }
    }
}
