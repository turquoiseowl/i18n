using System;
using System.Web;

namespace i18n
{
    public interface IEarlyUrlLocalizer
    {
        /// <summary>
        /// Method for performing Early Url Localization of the passed request.
        /// </summary>
        /// <param name="context">
        /// Current http context.
        /// </param>
        void ProcessIncoming(
            HttpContextBase context);

        /// <summary>
        /// Method for performing Late Url Localization of the passed response entity
        /// where the URLs in the entity may be amended with the passed langtag as appropriate.
        /// </summary>
        /// <param name="entity">
        /// Subject HTTP response entity to be processed.
        /// </param>
        /// <param name="langtag">
        /// Langtag to be patched into URLs.
        /// </param>
        /// <param name="context">
        /// Current http context.
        /// May be null if/when testing.
        /// </param>
        /// <returns>
        /// Processed (and possibly modified) entity.
        /// </returns>
        string ProcessOutgoing(
            string entity, 
            string langtag, 
            HttpContextBase context);
    }
}
