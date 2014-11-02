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
            ExtractLangTagFromUrlHelper("/zh"                      , "zh"                      , "/");
            ExtractLangTagFromUrlHelper("/zh-HK"                   , "zh-HK"                   , "/");
            ExtractLangTagFromUrlHelper("/zh-123"                  , "zh-123"                  , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans"                 , "zh-Hans"                 , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK"              , "zh-Hans-HK"              , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123"             , "zh-Hans-123"             , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD"      , "zh-Hans-123-x-ABCD"      , "/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123", "zh-Hans-123-x-ABCDEFG123", "/");

            ExtractLangTagFromUrlHelper("/zh/account"                      , "zh"                      , "/account");
            ExtractLangTagFromUrlHelper("/zh-HK/account"                   , "zh-HK"                   , "/account");
            ExtractLangTagFromUrlHelper("/zh-123/account"                  , "zh-123"                  , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans/account"                 , "zh-Hans"                 , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account"              , "zh-Hans-HK"              , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account"             , "zh-Hans-123"             , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account"      , "zh-Hans-123-x-ABCD"      , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account", "zh-Hans-123-x-ABCDEFG123", "/account");

            ExtractLangTagFromUrlHelper("/zh/account/"                      , "zh"                      , "/account/");
            ExtractLangTagFromUrlHelper("/zh-HK/account/"                   , "zh-HK"                   , "/account/");
            ExtractLangTagFromUrlHelper("/zh-123/account/"                  , "zh-123"                  , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/"                 , "zh-Hans"                 , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/"              , "zh-Hans-HK"              , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/"             , "zh-Hans-123"             , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account/"      , "zh-Hans-123-x-ABCD"      , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account/", "zh-Hans-123-x-ABCDEFG123", "/account/");

            ExtractLangTagFromUrlHelper("/zh/account/x"                      , "zh"                      , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-HK/account/x"                   , "zh-HK"                   , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-123/account/x"                  , "zh-123"                  , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/x"                 , "zh-Hans"                 , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/x"              , "zh-Hans-HK"              , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/x"             , "zh-Hans-123"             , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account/x"      , "zh-Hans-123-x-ABCD"      , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account/x", "zh-Hans-123-x-ABCDEFG123", "/account/x");

            ExtractLangTagFromUrlHelper("/azh"                      , null);
            ExtractLangTagFromUrlHelper("/azh-HK"                   , null);
            ExtractLangTagFromUrlHelper("/azh-123"                  , null);
            ExtractLangTagFromUrlHelper("/azh-Hans"                 , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-HK"              , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-123"             , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-123-x-ABCD"      , null);
            ExtractLangTagFromUrlHelper("/azh-Hans-123-x-ABCDEFG123", null);

            ExtractLangTagFromUrlHelper("/zh-a"                    , null);
            ExtractLangTagFromUrlHelper("/zh-aHK"                  , null);
            ExtractLangTagFromUrlHelper("/zh-a123"                 , null);
            ExtractLangTagFromUrlHelper("/zh-aHans"                , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-HK"             , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-123"            , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-123-x-ABCD"     , null);
            ExtractLangTagFromUrlHelper("/zh-aHans-HK-x-ABCDEFG123", null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x-ABC"        , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x-"           , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x"            , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-ABC"          , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-"             , null);

            ExtractLangTagFromUrlHelper("/zh-Hans-K"               , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-23"              , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-aHK"             , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-a123"            , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-H23"             , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12K"             , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12"              , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12-x-ABCD"       , null);
            ExtractLangTagFromUrlHelper("/zh-Hans-12-x-ABCDEFG123" , null);
        }

        private void MatchTagHelper1(
            int expected, 
            string lhs, 
            string rhs, 
            i18n.LanguageTag.MatchGrade matchGrade = i18n.LanguageTag.MatchGrade.LanguageMatch)
        {
            Assert.AreEqual(expected, (new i18n.LanguageTag(lhs).Match(new i18n.LanguageTag(rhs), matchGrade)));
            Assert.AreEqual(expected, (new i18n.LanguageTag(rhs).Match(new i18n.LanguageTag(lhs), matchGrade)));
        }
        private void MatchTagHelper(
            int expected, 
            string lhs, 
            string rhs, 
            i18n.LanguageTag.MatchGrade matchGrade = i18n.LanguageTag.MatchGrade.LanguageMatch)
        {
            MatchTagHelper1(expected, lhs, rhs, matchGrade);
           // If PrivateUse subtag not present in either tag...append such a subtag equally to both sides
           // and test again. This should have no effect on the result.
            if (-1 == lhs.IndexOf("-x-", StringComparison.OrdinalIgnoreCase) && -1 == rhs.IndexOf("-x-", StringComparison.OrdinalIgnoreCase)) {
                MatchTagHelper1(expected, lhs + "-x-abcd", rhs + "-x-abcd", matchGrade); }
        }

        [TestMethod]
        public void MatchTags()
        {
            // Test language tag matching and priority score

            // 100 (A)
            MatchTagHelper(100, "zh-Hans-HK", "zh-Hans-HK");
            MatchTagHelper(100, "zh-Hans"   , "zh-Hans");
            MatchTagHelper(100, "zh-HK"     , "zh-HK");
            MatchTagHelper(100, "zh"        , "zh");

            // 99 (B)
            MatchTagHelper(99 , "zh"        , "zh-HK");
            MatchTagHelper(99 , "zh-Hans"   , "zh-Hans-HK");

            // 98 (C)
            MatchTagHelper(98 , "zh-IK"     , "zh-HK");
            MatchTagHelper(98 , "zh-Hans-IK", "zh-Hans-HK");

            // 97 (D)
            MatchTagHelper(97 , "zh-HK"     , "zh-Hant-HK");
            MatchTagHelper(97 , "zh-HK"     , "zh-Hant-IK");
            MatchTagHelper(97 , "zh"        , "zh-Hant");
            MatchTagHelper(97 , "zh-HK"     , "zh-Hant");
            MatchTagHelper(97 , "zh"        , "zh-Hant-HK");

            // 96 (E)
            MatchTagHelper(96 , "zh-Hans-HK", "zh-Hant-HK");
            MatchTagHelper(96 , "zh-Hans-HK", "zh-Hant-IK");
            MatchTagHelper(96 , "zh-Hans"   , "zh-Hant");
            MatchTagHelper(96 , "zh-Hans-HK", "zh-Hant");
            MatchTagHelper(96 , "zh-Hans"   , "zh-Hant-HK");

            // 0 (F)
            MatchTagHelper(0  , "en-Hans-HK", "zh-Hans-HK");
            MatchTagHelper(0  , "en-Hans"   , "zh-Hans");
            MatchTagHelper(0  , "en-HK"     , "zh-HK");
            MatchTagHelper(0  , "en"        , "zh");
            MatchTagHelper(0  , "en"        , "zh-HK");
            MatchTagHelper(0  , "en-Hans"   , "zh-Hans-HK");
            MatchTagHelper(0  , "en-IK"     , "zh-HK");
            MatchTagHelper(0  , "en-Hans-IK", "zh-Hans-HK");
            MatchTagHelper(0  , "en-HK"     , "zh-Hant-HK");
            MatchTagHelper(0  , "en-HK"     , "zh-Hant-IK");
            MatchTagHelper(0  , "en"        , "zh-Hant");
            MatchTagHelper(0  , "en-HK"     , "zh-Hant");
            MatchTagHelper(0  , "en"        , "zh-Hant-HK");
            MatchTagHelper(0  , "en-Hans-HK", "zh-Hant-HK");
            MatchTagHelper(0  , "en-Hans-HK", "zh-Hant-IK");
            MatchTagHelper(0  , "en-Hans"   , "zh-Hant");
            MatchTagHelper(0  , "en-Hans-HK", "zh-Hant");
                // 

            // 0 (G)
            MatchTagHelper(0  , "zh"               , "zh-x-efgh");
            MatchTagHelper(0  , "zh-HK"            , "zh-HK-x-efgh");
            MatchTagHelper(0  , "zh-Hans-HK"       , "zh-Hans-HK-x-efgh");
            MatchTagHelper(0  , "zh-x-abcd"        , "zh-x-efgh");
            MatchTagHelper(0  , "zh-HK-x-abcd"     , "zh-HK-x-efgh");
            MatchTagHelper(0  , "zh-Hans-HK-x-abcd", "zh-Hans-HK-x-efgh");
                // PrivateUse subtag mismatch, but other tags match.
            MatchTagHelper(0  , "zh-x-abcd"        , "zh-x-efgh");
            MatchTagHelper(0  , "zh-IK-x-abcd"     , "zh-x-efgh");
            MatchTagHelper(0  , "zh-Hans-x-abcd"   , "zh-x-efgh");
            MatchTagHelper(0  , "zh-Hans-HK-x-abcd", "zh-x-efgh");
            MatchTagHelper(0  , "zh-x-abcd"        , "zh-HK-x-efgh");
            MatchTagHelper(0  , "zh-IK-x-abcd"     , "zh-HK-x-efgh");
            MatchTagHelper(0  , "zh-Hans-x-abcd"   , "zh-HK-x-efgh");
            MatchTagHelper(0  , "zh-Hans-HK-x-abcd", "zh-HK-x-efgh");
            MatchTagHelper(0  , "zh-x-abcd"        , "zh-Hant-x-efgh");
            MatchTagHelper(0  , "zh-IK-x-abcd"     , "zh-Hant-x-efgh");
            MatchTagHelper(0  , "zh-Hans-x-abcd"   , "zh-Hant-x-efgh");
            MatchTagHelper(0  , "zh-Hans-HK-x-abcd", "zh-Hant-x-efgh");
                // Mismatch in PrivateUse and other subtags.
        }
    }
}
