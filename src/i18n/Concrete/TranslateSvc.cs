using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using i18n;

namespace i18n
{
    /// <summary>
    /// ITranslateSvc implementation based on an given HttpContextBase instance.
    /// </summary>
    public class TranslateSvc_HttpContextBase : ITranslateSvc
    {

    #region ITranslateSvc

        public string ParseAndTranslate(string entity)
        {
            return m_context.ParseAndTranslate(entity);
        }

    #endregion

        HttpContextBase m_context;

        public TranslateSvc_HttpContextBase(HttpContextBase context)
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

        HttpContext m_context;

        public TranslateSvc_HttpContext(HttpContext context)
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
            return HttpContext.Current.ParseAndTranslate(entity);
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
