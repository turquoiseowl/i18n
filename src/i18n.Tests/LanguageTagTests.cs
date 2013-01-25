using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace i18n.Tests
{
    [TestFixture]
    public class LanguageTag
    {
        private void UrlExtractLangTagHelper(string url, string expectedLangTag, string expectedUrlPatched)
        {
            string urlPatched;
            i18n.LanguageTag lt = i18n.LanguageTag.UrlExtractLangTag(url, out urlPatched);
            string langtag = lt != null ? lt.ToString() : null;
            Assert.AreEqual(expectedLangTag, langtag);
            Assert.AreEqual(expectedUrlPatched, urlPatched);
        }

        [Test]
        public void UrlExtractLangTag()
        {
            UrlExtractLangTagHelper("/zh"         , "zh"         , "/");
            UrlExtractLangTagHelper("/zh-HK"      , "zh-HK"      , "/");
            UrlExtractLangTagHelper("/zh-123"     , "zh-123"     , "/");
            UrlExtractLangTagHelper("/zh-Hans"    , "zh-Hans"    , "/");
            UrlExtractLangTagHelper("/zh-Hans-HK" , "zh-Hans-HK" , "/");
            UrlExtractLangTagHelper("/zh-Hans-123", "zh-Hans-123", "/");

            UrlExtractLangTagHelper("/zh/account"         , "zh"         , "/account");
            UrlExtractLangTagHelper("/zh-HK/account"      , "zh-HK"      , "/account");
            UrlExtractLangTagHelper("/zh-123/account"     , "zh-123"     , "/account");
            UrlExtractLangTagHelper("/zh-Hans/account"    , "zh-Hans"    , "/account");
            UrlExtractLangTagHelper("/zh-Hans-HK/account" , "zh-Hans-HK" , "/account");
            UrlExtractLangTagHelper("/zh-Hans-123/account", "zh-Hans-123", "/account");

            UrlExtractLangTagHelper("/zh/account/"         , "zh"         , "/account/");
            UrlExtractLangTagHelper("/zh-HK/account/"      , "zh-HK"      , "/account/");
            UrlExtractLangTagHelper("/zh-123/account/"     , "zh-123"     , "/account/");
            UrlExtractLangTagHelper("/zh-Hans/account/"    , "zh-Hans"    , "/account/");
            UrlExtractLangTagHelper("/zh-Hans-HK/account/" , "zh-Hans-HK" , "/account/");
            UrlExtractLangTagHelper("/zh-Hans-123/account/", "zh-Hans-123", "/account/");

            UrlExtractLangTagHelper("/zh/account/x"         , "zh"         , "/account/x");
            UrlExtractLangTagHelper("/zh-HK/account/x"      , "zh-HK"      , "/account/x");
            UrlExtractLangTagHelper("/zh-123/account/x"     , "zh-123"     , "/account/x");
            UrlExtractLangTagHelper("/zh-Hans/account/x"    , "zh-Hans"    , "/account/x");
            UrlExtractLangTagHelper("/zh-Hans-HK/account/x" , "zh-Hans-HK" , "/account/x");
            UrlExtractLangTagHelper("/zh-Hans-123/account/x", "zh-Hans-123", "/account/x");

            UrlExtractLangTagHelper("/azh"         , null         , null);
            UrlExtractLangTagHelper("/azh-HK"      , null         , null);
            UrlExtractLangTagHelper("/azh-123"     , null         , null);
            UrlExtractLangTagHelper("/azh-Hans"    , null         , null);
            UrlExtractLangTagHelper("/azh-Hans-HK" , null         , null);
            UrlExtractLangTagHelper("/azh-Hans-123", null         , null);

            UrlExtractLangTagHelper("/zh-a"        , null         , null);
            UrlExtractLangTagHelper("/zh-aHK"      , null         , null);
            UrlExtractLangTagHelper("/zh-a123"     , null         , null);
            UrlExtractLangTagHelper("/zh-aHans"    , null         , null);
            UrlExtractLangTagHelper("/zh-aHans-HK" , null         , null);
            UrlExtractLangTagHelper("/zh-aHans-123", null         , null);

            UrlExtractLangTagHelper("/zh-Hans-K"   , null         , null);
            UrlExtractLangTagHelper("/zh-Hans-23"  , null         , null);
            UrlExtractLangTagHelper("/zh-Hans-aHK" , null         , null);
            UrlExtractLangTagHelper("/zh-Hans-a123", null         , null);
            UrlExtractLangTagHelper("/zh-Hans-H23" , null         , null);
            UrlExtractLangTagHelper("/zh-Hans-12K" , null         , null);
            UrlExtractLangTagHelper("/zh-Hans-12"  , null         , null);
        }
    }
}
