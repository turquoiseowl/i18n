using System;

namespace i18n
{
    /// <summary>
    /// ITranslateSvc implementation based on an given System.Web.HttpContextBase instance.
    /// </summary>
    public class TranslateSvc_HttpContextBase : ITranslateSvc
    {

    #region ITranslateSvc

        public string ParseAndTranslate(string entity)
        {
            return m_context.ParseAndTranslate(entity);
        }

    #endregion

        System.Web.HttpContextBase m_context;

        public TranslateSvc_HttpContextBase(System.Web.HttpContextBase context)
        {
            if (context == null) { throw new ArgumentNullException("context"); }

            m_context = context;
        }

    }

    /// <summary>
    /// ITranslateSvc implementation based on an given HttpContext instance.
    /// </summary>
    public class TranslateSvc_HttpContext : ITranslateSvc
    {

    #region ITranslateSvc

        public string ParseAndTranslate(string entity)
        {
            return m_context.ParseAndTranslate(entity);
        }

    #endregion

        System.Web.HttpContext m_context;

        public TranslateSvc_HttpContext(System.Web.HttpContext context)
        {
            if (context == null) { throw new ArgumentNullException("context"); }

            m_context = context;
        }

    }

    /// <summary>
    /// ITranslateSvc implementation based on the static HttpContext.Current instance.
    /// </summary>
    public class TranslateSvc_HttpContextCurrent : ITranslateSvc
    {

    #region ITranslateSvc

        public string ParseAndTranslate(string entity)
        {
            return System.Web.HttpContext.Current.ParseAndTranslate(entity);
        }

    #endregion

    }

    /// <summary>
    /// ITranslateSvc implementation that simply passes through the entity (useful for testing).
    /// </summary>
    public class TranslateSvc_Invariant : ITranslateSvc
    {

    #region ITranslateSvc

        public string ParseAndTranslate(string entity)
        {
            return entity;
        }

    #endregion

    }
}
