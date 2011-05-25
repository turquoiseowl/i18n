using System;
using Microsoft.Practices.ServiceLocation;

namespace i18n
{
    internal class DependencyResolver
    {
        #region ILocalizingService
        private static ILocalizingService _defaultLocalizingService;
        public static ILocalizingService LocalizingService
        {
            get
            {
                if (_defaultLocalizingService != null)
                {
                    return _defaultLocalizingService;
                }
                var locator = TryGetLocator();
                return locator == null ? DefaultLocalizingService : TryGetInstance<ILocalizingService>(locator) ?? DefaultLocalizingService;
            }
        }
        private static ILocalizingService DefaultLocalizingService
        {
            get { return _defaultLocalizingService ?? (_defaultLocalizingService = new LocalizingService()); }
        } 
        #endregion

        private static IServiceLocator TryGetLocator()
        {
            try
            {
                return ServiceLocator.Current;
            }
            catch(NullReferenceException)
            {
                return null;
            }
        }

        private static T TryGetInstance<T>(IServiceLocator locator)
        {
            try
            {
                var instance = locator.GetInstance<T>();
                return instance;
            }
            catch (ActivationException)
            {
                return default(T);
            }
        }
    }
}