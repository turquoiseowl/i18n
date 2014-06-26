using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace i18n.Tests
{
    [TestClass]
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

        [TestMethod]
        public void ExtractLangTagFromUrl()
        {
            ExtractLangTagFromUrlHelper("/zh"         , "zh"         , "/");
            ExtractLangTagFromUrlHelper("/zh-HK"      , "zh-HK"      , "/");
            ExtractLangTagFromUrlHelper("/zh-123"     , "zh-123"     , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans"    , "zh-Hans"    , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK" , "zh-Hans-HK" , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123", "zh-Hans-123", "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD", "zh-Hans-123-x-ABCD", "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123", "zh-Hans-123-x-ABCDEFG123", "/");

            ExtractLangTagFromUrlHelper("/zh/account"         , "zh"         , "/account");
            ExtractLangTagFromUrlHelper("/zh-HK/account"      , "zh-HK"      , "/account");
            ExtractLangTagFromUrlHelper("/zh-123/account"     , "zh-123"     , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans/account"    , "zh-Hans"    , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account" , "zh-Hans-HK" , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account", "zh-Hans-123", "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account", "zh-Hans-123-x-ABCD", "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account", "zh-Hans-123-x-ABCDEFG123", "/account");

            ExtractLangTagFromUrlHelper("/zh/account/"         , "zh"         , "/account/");
            ExtractLangTagFromUrlHelper("/zh-HK/account/"      , "zh-HK"      , "/account/");
            ExtractLangTagFromUrlHelper("/zh-123/account/"     , "zh-123"     , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/"    , "zh-Hans"    , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/" , "zh-Hans-HK" , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/", "zh-Hans-123", "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account/", "zh-Hans-123-x-ABCD", "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account/", "zh-Hans-123-x-ABCDEFG123", "/account/");

            ExtractLangTagFromUrlHelper("/zh/account/x"         , "zh"         , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-HK/account/x"      , "zh-HK"      , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-123/account/x"     , "zh-123"     , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/x"    , "zh-Hans"    , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/x" , "zh-Hans-HK" , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/x", "zh-Hans-123", "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account/x", "zh-Hans-123-x-ABCD", "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account/x", "zh-Hans-123-x-ABCDEFG123", "/account/x");

            ExtractLangTagFromUrlHelper("/azh"         , null);
            ExtractLangTagFromUrlHelper("/azh-HK"      , null);
            ExtractLangTagFromUrlHelper("/azh-123"     , null);
            ExtractLangTagFromUrlHelper("/azh-Hans"    , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-HK" , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-123", null);
            ExtractLangTagFromUrlHelper("/azh-Hans-123-x-ABCD", null);
            ExtractLangTagFromUrlHelper("/azh-Hans-123-x-ABCDEFG123", null);

            ExtractLangTagFromUrlHelper("/zh-a"        , null);
            ExtractLangTagFromUrlHelper("/zh-aHK"      , null);
            ExtractLangTagFromUrlHelper("/zh-a123"     , null);
            ExtractLangTagFromUrlHelper("/zh-aHans"    , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-HK" , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-123", null);
            ExtractLangTagFromUrlHelper("/zh-aHans-123-x-ABCD", null);
            ExtractLangTagFromUrlHelper("/zh-aHans-HK-x-ABCDEFG123", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x-ABC", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x-", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-ABC", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-", null);

            ExtractLangTagFromUrlHelper("/zh-Hans-K"   , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-23"  , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-aHK" , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-a123", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-H23" , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12K" , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12"  , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12-x-ABCD", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12-x-ABCDEFG123", null);
        }

        private int MatchTagHelper(string lhs, string rhs)
        {
            return (new i18n.LanguageTag(lhs).Match(new i18n.LanguageTag(rhs)));
        }

        [TestMethod]
        public void MatchTags()
        {

            // Test language tag matching and priority score
            //100 
            Assert.AreEqual(MatchTagHelper("en-aaaa-us-x-abcd","en-aaaa-us-x-abcd"),100);
            Assert.AreEqual(MatchTagHelper("en-us-x-abcd","en-us-x-abcd"),100);
            Assert.AreEqual(MatchTagHelper("en-x-abcd","en-x-abcd"),100);
            //99
            Assert.AreEqual(MatchTagHelper("en-aaaa-us-x-abcd","en-aaaa-us"),99);
            Assert.AreEqual(MatchTagHelper("en-aaaa-us","en-aaaa-us"),99);
            Assert.AreEqual(MatchTagHelper("en-us","en-us"),99);
            Assert.AreEqual(MatchTagHelper("en","en"),99);
            //98
            Assert.AreEqual(MatchTagHelper("en-aaaa-x-abcd","en-aaaa-us"),98);
            Assert.AreEqual(MatchTagHelper("en-aaaa","en-aaaa-us"),98);
            Assert.AreEqual(MatchTagHelper("en","en-us"),98);
            //97
            Assert.AreEqual(MatchTagHelper("en-aaaa-gb-x-abcd","en-aaaa-us"),97);
            Assert.AreEqual(MatchTagHelper("en-aaaa-gb","en-aaaa-us"),97);
            Assert.AreEqual(MatchTagHelper("en-gb","en-us"),97);
            //96
            Assert.AreEqual(MatchTagHelper("en-us-x-abcd","en-aaaa-us-x-abcd"),96);
            Assert.AreEqual(MatchTagHelper("en-us-x-abcd","en-aaaa-us"),96);
            Assert.AreEqual(MatchTagHelper("en-us","en-aaaa-us"),96);
            //95
            Assert.AreEqual(MatchTagHelper("en-bbbb-x-abcd","en-aaaa-us"),95);
            Assert.AreEqual(MatchTagHelper("en-bbbb","en-aaaa-us"),95);
            //0
            Assert.AreEqual(MatchTagHelper("en","de"),0);
            Assert.AreEqual(MatchTagHelper("en-GB","de-GB"),0);
            Assert.AreEqual(MatchTagHelper("en-x-abcd","de-x-abcd"),0);
            Assert.AreEqual(MatchTagHelper("en-x-abcd","en-x-xxxx"),0);

        }
    }
}
