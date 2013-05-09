using System;
using System.Threading;
using System.Web;
using System.Text.RegularExpressions;
using container;
using i18n.Domain.Helpers;
using i18n.Domain.Abstract;
using i18n.Domain.Concrete;

namespace i18n
{
    /// <summary>
    /// Manages the configuration of the i18n features of your localized application.
    /// </summary>
    public class LocalizedApplication : IRootServices
    {

    #region [IApplicationServices]

        public ITextLocalizer TextLocalizerForApp
        {
            get {
                return m_cached_textLocalizer.Get(() => TextLocalizerService);
            }
        }
        public IEarlyUrlLocalizer EarlyUrlLocalizerForApp
        {
            get {
                return m_cached_earlyUrlLocalizer.Get(() => EarlyUrlLocalizerService);
            }
        }
        public INuggetLocalizer NuggetLocalizerForApp
        {
            get {
                return m_cached_nuggetLocalizer.Get(() => NuggetLocalizerService);
            }
        }

    #endregion

        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location.
        /// </summary>
        /// <remarks>
        /// Supports a subset of BCP 47 language tag spec corresponding to the Windows
        /// support for language names, namely the following subtags:
        ///     language (mandatory, 2 alphachars)
        ///     script   (optional, 4 alphachars)
        ///     region   (optional, 2 alphachars | 3 decdigits)
        /// Example tags supported:
        ///     "en"            [language]
        ///     "en-US"         [language + region]
        ///     "zh"            [language]
        ///     "zh-HK"         [language + region]
        ///     "zh-123"        [language + region]
        ///     "zh-Hant"       [language + script]
        ///     "zh-Hant-HK"    [language + script + region]
        /// </remarks>
        public string DefaultLanguage { 
            get {
                return DefaultLanguageTag.ToString();
            }
            set {
                DefaultLanguageTag = LanguageTag.GetCachedInstance(value);
            }
        }
        public LanguageTag DefaultLanguageTag { get; set; }

        /// <summary>
        /// Specifies the root path for the site for correct URL Localization.
        /// </summary>
        /// <remarks>
        /// Where the root application path for your site maps to the root of the URL,
        /// leave this as null.<br/>
        /// Where the root application path for your site maps to the sub path in the URL,
        /// set this to that sub path part.<br/>
        /// E.g. if the application root url is "example.com/MySite",
        /// set this to "/MySite". It is important that the string starts with a forward slash path separator
        /// and does NOT end with a forward slash.
        /// </remarks>
        public string SiteRootPath { get; set; }

        /// <summary>
        /// Declares a method type for handling the setting of the language.
        /// </summary>
        /// <param name="context">Current http context.</param>
        /// <param name="langtag">Language being set.</param>
        public delegate void SetLanguageHandler(HttpContextBase context, ILanguageTag langtag);

        /// <summary>
        /// Describes one or more procedures to be called when the principal application
        /// language (PAL) is set for an HTTP request.
        /// </summary>
        /// <remarks>
        /// A default handlers is installed which applies the PAL setting to both the 
        /// CurrentCulture and CurrentUICulture settings of the current thread.
        /// This behaviour can be altered by removing (nulling) the value of this property
        /// or replacing with a new delegate.
        /// </remarks>
        public SetLanguageHandler SetPrincipalAppLanguageForRequestHandlers { get; set; }

        /// <summary>
        /// Specifies the type of HTTP redirect to be issued by automatic language routing:
        /// true for 301 (permanent) redirects; false for 302 (temporary) ones.
        /// Defaults to false.
        /// </summary>
        public bool PermanentRedirects { get; set; }

        /// <summary>
        /// Regular expression that controls the ContextTypes elligible for Late URL Localization.
        /// </summary>
        /// <remarks>
        /// Set to null to disable Late URL Localization. Defaults to text/html and 
        /// application/javascript. Client may customise this member, for instance in Application_Start.
        /// This feature requires the LocalizedModule HTTP module to be intalled in web.config.
        /// </remarks>
        public Regex ContentTypesToLocalize = new Regex("^(?:text/html|application/javascript)$");

        public LocalizedApplication()
        {
            Container = new Container();

            // Default settings.
            DefaultLanguage = ("en");
            PermanentRedirects = false;
            SiteRootPath = null;

            // Register default services.
            // The client app may subsequerntly override any of these.
            // NB: we are registering the factory functions/delegates here, not actually
            // creating the services instances.
            Container.Register<ITranslationRepository>(r => new POTranslationRepository(new i18nSettings(new WebConfigSettingService(null)))); 
            Container.Register<IUrlLocalizer>(r => new UrlLocalizer());
            Container.Register<ITextLocalizer>(r => new TextLocalizer(TranslationRepositoryService)); 
            Container.Register<IEarlyUrlLocalizer>(r => new EarlyUrlLocalizer(UrlLocalizerService));
            Container.Register<INuggetLocalizer>(r => new NuggetLocalizer(TextLocalizerForApp));
                // TextLocalizerForApp = re-use any cached TextLocalizer already instantiated.
                // This prevents NuggetLocalizer using a different TextLocalizer instance from
                // a client which calls TextLocalizerForApp directly. While it is dangerous to do this
                // from a DI perspective, it should be okay because any service factory change resets/clears any
                // cached service instances.

            // Install default handler for Set-PAL event.
            // The default handler applies the setting to both the CurrentCulture and CurrentUICulture
            // settings of the thread.
            SetPrincipalAppLanguageForRequestHandlers = delegate(HttpContextBase context, ILanguageTag langtag)
            {
                if (langtag != null) {
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = langtag.GetCultureInfo(); }
            };
        }

        /// <summary>
        /// Instance of the this LocalizedApplication class for the current AppDomain.
        /// </summary>
        public static LocalizedApplication Current = new LocalizedApplication();
        
        private Container Container { get; set; }
        private LockFreeProperty<ITextLocalizer> m_cached_textLocalizer = new LockFreeProperty<ITextLocalizer>();
        private LockFreeProperty<IEarlyUrlLocalizer> m_cached_earlyUrlLocalizer = new LockFreeProperty<IEarlyUrlLocalizer>();
        private LockFreeProperty<INuggetLocalizer> m_cached_nuggetLocalizer = new LockFreeProperty<INuggetLocalizer>();

        /// <summary>
        /// Helper for clearing the cached-allocated per-appdomain services maintained by this class.
        /// Typically we want to do this when changing the type of one or more of the dependents of these services.
        /// </summary>
        private void ResetCachedServices()
        {
            m_cached_textLocalizer.Reset();
            m_cached_earlyUrlLocalizer.Reset();
            m_cached_nuggetLocalizer.Reset();
        }

        /// <summary>
        /// Gets or sets an instance to use for the namesake service type.
        /// </summary>
        /// <remarks>
        /// Setting this interface implicity enables or disables the respective feacture.
        /// This feature depends on the LocalizedModule HTTP module being enabled in web.config.
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public ITranslationRepository TranslationRepositoryService
        {
            get { return Container.Resolve<ITranslationRepository>(); }
            set
            {
               // Reset/clear any antecendents that may be using the previous service.
                ResetCachedServices();
               //
                Container.Remove<ITranslationRepository>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// Gets or sets an instance to use for the namesake service type.
        /// </summary>
        /// <remarks>
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public ITextLocalizer TextLocalizerService
        {
            get { return Container.Resolve<ITextLocalizer>(); }
            set
            {
               // Reset/clear any antecendents that may be using the previous service.
                ResetCachedServices();
                //
                Container.Remove<ITextLocalizer>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// Gets or sets an instance to use for the namesake service type.
        /// </summary>
        /// <remarks>
        /// Setting this interface implicity enables or disables the respective feature.
        /// This feature depends on the LocalizedModule HTTP module being enabled in web.config.
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public IEarlyUrlLocalizer EarlyUrlLocalizerService
        {
            get { return Container.Resolve<IEarlyUrlLocalizer>(); }
            set
            {
               // Reset/clear any antecendents that may be using the previous service.
                ResetCachedServices();
               //
                Container.Remove<IEarlyUrlLocalizer>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// Gets or sets an instance to use for the namesake service type.
        /// </summary>
        /// <remarks>
        /// Setting this interface implicity enables or disables the respective feature.
        /// This feature depends on the LocalizedModule HTTP module being enabled in web.config.
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public INuggetLocalizer NuggetLocalizerService
        {
            get { return Container.Resolve<INuggetLocalizer>(); }
            set
            {
               // Reset/clear any antecendents that may be using the previous service.
                ResetCachedServices();
               //
                Container.Remove<INuggetLocalizer>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// Gets or sets an instance to use for the namesake service type.
        /// </summary>
        /// <remarks>
        /// This interface is used by the default EarlyUrlLocalizer and NuggetLocalizer implementations.
        /// [Deprecated] It is also used by the MVC RouteLocalization implementation.
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public IUrlLocalizer UrlLocalizerService
        {
            get { return Container.Resolve<IUrlLocalizer>(); }
            set
            {
               // Reset/clear any antecendents that may be using the previous service.
                ResetCachedServices();
               //
                Container.Remove<IUrlLocalizer>();
                Container.Register(r => value);
            }
        }
    }
}
