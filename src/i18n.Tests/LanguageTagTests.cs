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
            string langtag = i18n.LanguageTag.UrlExtractLangTag(url, out urlPatched);
            Assert.AreEqual(expectedLangTag, langtag);
            Assert.AreEqual(expectedUrlPatched, urlPatched);
        }
        private void UrlExtractLangTagHelper(string url, string expectedLangTag)
        {
            UrlExtractLangTagHelper(url, expectedLangTag, url);
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

            UrlExtractLangTagHelper("/azh"         , null);
            UrlExtractLangTagHelper("/azh-HK"      , null);
            UrlExtractLangTagHelper("/azh-123"     , null);
            UrlExtractLangTagHelper("/azh-Hans"    , null);
            UrlExtractLangTagHelper("/azh-Hans-HK" , null);
            UrlExtractLangTagHelper("/azh-Hans-123", null);

            UrlExtractLangTagHelper("/zh-a"        , null);
            UrlExtractLangTagHelper("/zh-aHK"      , null);
            UrlExtractLangTagHelper("/zh-a123"     , null);
            UrlExtractLangTagHelper("/zh-aHans"    , null);
            UrlExtractLangTagHelper("/zh-aHans-HK" , null);
            UrlExtractLangTagHelper("/zh-aHans-123", null);

            UrlExtractLangTagHelper("/zh-Hans-K"   , null);
            UrlExtractLangTagHelper("/zh-Hans-23"  , null);
            UrlExtractLangTagHelper("/zh-Hans-aHK" , null);
            UrlExtractLangTagHelper("/zh-Hans-a123", null);
            UrlExtractLangTagHelper("/zh-Hans-H23" , null);
            UrlExtractLangTagHelper("/zh-Hans-12K" , null);
            UrlExtractLangTagHelper("/zh-Hans-12"  , null);
        }
    }
}
