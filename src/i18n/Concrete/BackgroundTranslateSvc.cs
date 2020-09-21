using System;

namespace i18n
{
    /// <summary>
    /// Default i18n IBackgroundTranslateSvc implementation that simply
    /// relays calls on to the i18n API.
    /// </summary>
    public class BackgroundTranslateSvc_Default : IBackgroundTranslateSvc
    {

    #region IBackgroundTranslateSvc

        public string ParseAndTranslate(string entity, string userLanguages = null)
        {
            return LanguageHelpers.ParseAndTranslate(entity, userLanguages);
        }

    #endregion

    }

    /// <summary>
    /// IBackgroundTranslateSvc implementation that simply passes through the entity (useful for testing).
    /// </summary>
    public class BackgroundTranslateSvc_Invariant : IBackgroundTranslateSvc
    {

    #region IBackgroundTranslateSvc

        public string ParseAndTranslate(string entity, string userLanguages)
        {
            return entity;
        }

    #endregion

    }
}
