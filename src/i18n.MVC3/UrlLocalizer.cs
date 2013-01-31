using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n
{
    public class UrlLocalizer : IUrlLocalizer
    {
    #region IUrlLocalizer
        public string ExtractLangTagFromUrl(string url, out string urlPatched)
        {
            return LanguageTag.UrlExtractLangTag(url, out urlPatched);
        }
        public string SetLangTagInUrl(string url, string langtag)
        {
            return LanguageTag.UrlSetLangTag(url, langtag).ToString();
        }
        public string InsertLangTagIntoVirtualPath(string langtag, string virtualPath)
        {
            // Prepend the virtual path with the PAL langtag.
            // E.g. "account/signup" -> "fr-CH/account/signup"
            // E.g. ""               -> "fr-CH"
            return virtualPath.IsSet() ? string.Format("{0}/{1}", langtag, virtualPath) : langtag;
        }
    #endregion
    }
}
