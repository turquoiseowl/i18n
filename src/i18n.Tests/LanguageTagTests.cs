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
        private void ExtractLangTagFromUrlHelper(string url, string expectedLangTag, string expectedUrlPatched)
        {
            string urlPatched;
            string langtag;
            
            langtag = i18n.LanguageTag.ExtractLangTagFromUrl(url, UriKind.Relative, out urlPatched);
            Assert.AreEqual(expectedLangTag, langtag);
            Assert.AreEqual(expectedUrlPatched, urlPatched);

            langtag = i18n.LanguageTag.ExtractLangTagFromUrl(url, UriKind.RelativeOrAbsolute, out urlPatched);
            Assert.AreEqual(expectedLangTag, langtag);
            Assert.AreEqual(expectedUrlPatched, urlPatched);
        }
        private void ExtractLangTagFromUrlHelper(string url, string expectedLangTag)
        {
            ExtractLangTagFromUrlHelper(url, expectedLangTag, url);
        }

        [Test]
        public void ExtractLangTagFromUrl()
        {
            ExtractLangTagFromUrlHelper("/zh"         , "zh"         , "/");
            ExtractLangTagFromUrlHelper("/zh-HK"      , "zh-HK"      , "/");
            ExtractLangTagFromUrlHelper("/zh-123"     , "zh-123"     , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans"    , "zh-Hans"    , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK" , "zh-Hans-HK" , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123", "zh-Hans-123", "/");

            ExtractLangTagFromUrlHelper("/zh/account"         , "zh"         , "/account");
            ExtractLangTagFromUrlHelper("/zh-HK/account"      , "zh-HK"      , "/account");
            ExtractLangTagFromUrlHelper("/zh-123/account"     , "zh-123"     , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans/account"    , "zh-Hans"    , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account" , "zh-Hans-HK" , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account", "zh-Hans-123", "/account");

            ExtractLangTagFromUrlHelper("/zh/account/"         , "zh"         , "/account/");
            ExtractLangTagFromUrlHelper("/zh-HK/account/"      , "zh-HK"      , "/account/");
            ExtractLangTagFromUrlHelper("/zh-123/account/"     , "zh-123"     , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/"    , "zh-Hans"    , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/" , "zh-Hans-HK" , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/", "zh-Hans-123", "/account/");

            ExtractLangTagFromUrlHelper("/zh/account/x"         , "zh"         , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-HK/account/x"      , "zh-HK"      , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-123/account/x"     , "zh-123"     , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/x"    , "zh-Hans"    , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/x" , "zh-Hans-HK" , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/x", "zh-Hans-123", "/account/x");

            ExtractLangTagFromUrlHelper("/azh"         , null);
            ExtractLangTagFromUrlHelper("/azh-HK"      , null);
            ExtractLangTagFromUrlHelper("/azh-123"     , null);
            ExtractLangTagFromUrlHelper("/azh-Hans"    , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-HK" , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-123", null);

            ExtractLangTagFromUrlHelper("/zh-a"        , null);
            ExtractLangTagFromUrlHelper("/zh-aHK"      , null);
            ExtractLangTagFromUrlHelper("/zh-a123"     , null);
            ExtractLangTagFromUrlHelper("/zh-aHans"    , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-HK" , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-123", null);

            ExtractLangTagFromUrlHelper("/zh-Hans-K"   , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-23"  , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-aHK" , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-a123", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-H23" , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12K" , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12"  , null);
        }
    }
}
