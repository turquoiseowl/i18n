namespace i18n
{
    public class DependencyResolver
    {
        public static IocContainer Container { get; set; }
        static DependencyResolver()
        {
            Container = new IocContainer();
            Container.Register<ILocalizingService>(r => new LocalizingService());
        }
        public static IHtmlStringFormatter HtmlStringFormatter
        {
            get { return Container.Resolve<IHtmlStringFormatter>(); }
        }
        public static ILocalizingService LocalizingService
        {
            get { return Container.Resolve<ILocalizingService>(); }
        }
    }
}