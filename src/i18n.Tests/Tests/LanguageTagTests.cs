using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace i18n.Tests
{
    [TestClass]
    public class LanguageTagTests
    {
        private void ExtractLangTagFromUrlHelper_Absolute(string url, string expectedLangTag, string expectedUrlPatched)
        {
            string urlPatched;
            string langtag;
            
            langtag = i18n.LanguageTag.ExtractLangTagFromUrl(url, UriKind.Absolute, out urlPatched);
            Assert.AreEqual(expectedLangTag, langtag);
            Assert.AreEqual(expectedUrlPatched, urlPatched);

            langtag = i18n.LanguageTag.ExtractLangTagFromUrl(url, UriKind.RelativeOrAbsolute, out urlPatched);
            Assert.AreEqual(expectedLangTag, langtag);
            Assert.AreEqual(expectedUrlPatched, urlPatched);
        }
        private void ExtractLangTagFromUrlHelper_Relative(string url, string expectedLangTag, string expectedUrlPatched)
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
        private void ExtractLangTagFromUrlHelper_Relative(string url, string expectedLangTag)
        {
            ExtractLangTagFromUrlHelper_Relative(url, expectedLangTag, url);
        }

        [TestMethod]
        public void ExtractLangTagFromUrl_Absolute()
        {
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh/account"                      , "zh"                      , "https://example.com/account");
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh-HK/account"                   , "zh-HK"                   , "https://example.com/account");
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh-123/account"                  , "zh-123"                  , "https://example.com/account");
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh-Hans/account"                 , "zh-Hans"                 , "https://example.com/account");
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh-Hans-HK/account"              , "zh-Hans-HK"              , "https://example.com/account");
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh-Hans-123/account"             , "zh-Hans-123"             , "https://example.com/account");
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh-Hans-123-x-ABCD/account"      , "zh-Hans-123-x-ABCD"      , "https://example.com/account");
            ExtractLangTagFromUrlHelper_Absolute("https://example.com/zh-Hans-123-x-ABCDEFG123/account", "zh-Hans-123-x-ABCDEFG123", "https://example.com/account");
        }

        [TestMethod]
        public void ExtractLangTagFromUrl_Relative()
        {
            ExtractLangTagFromUrlHelper_Relative("/zh"                      , "zh"                      , "/");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK"                   , "zh-HK"                   , "/");
            ExtractLangTagFromUrlHelper_Relative("/zh-123"                  , "zh-123"                  , "/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans"                 , "zh-Hans"                 , "/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK"              , "zh-Hans-HK"              , "/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123"             , "zh-Hans-123"             , "/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD"      , "zh-Hans-123-x-ABCD"      , "/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123", "zh-Hans-123-x-ABCDEFG123", "/");

            // #383
            ExtractLangTagFromUrlHelper_Relative("/zh?"                        , "zh"                      , "/?");
            ExtractLangTagFromUrlHelper_Relative("/zh?qs"                      , "zh"                      , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK?qs"                   , "zh-HK"                   , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-123?qs"                  , "zh-123"                  , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans?qs"                 , "zh-Hans"                 , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK?qs"              , "zh-Hans-HK"              , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123?qs"             , "zh-Hans-123"             , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD?qs"      , "zh-Hans-123-x-ABCD"      , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123?qs", "zh-Hans-123-x-ABCDEFG123", "/?qs");

            ExtractLangTagFromUrlHelper_Relative("/zh#"                        , "zh"                      , "/#");
            ExtractLangTagFromUrlHelper_Relative("/zh#bm"                      , "zh"                      , "/#bm");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK#bm"                   , "zh-HK"                   , "/#bm");
            ExtractLangTagFromUrlHelper_Relative("/zh-123#bm"                  , "zh-123"                  , "/#bm");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans#bm"                 , "zh-Hans"                 , "/#bm");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK#bm"              , "zh-Hans-HK"              , "/#bm");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123#bm"             , "zh-Hans-123"             , "/#bm");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD#bm"      , "zh-Hans-123-x-ABCD"      , "/#bm");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123#bm", "zh-Hans-123-x-ABCDEFG123", "/#bm");

            ExtractLangTagFromUrlHelper_Relative("/zh/?qs"                      , "zh"                      , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK/?qs"                   , "zh-HK"                   , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-123/?qs"                  , "zh-123"                  , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans/?qs"                 , "zh-Hans"                 , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK/?qs"              , "zh-Hans-HK"              , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123/?qs"             , "zh-Hans-123"             , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD/?qs"      , "zh-Hans-123-x-ABCD"      , "/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123/?qs", "zh-Hans-123-x-ABCDEFG123", "/?qs");

            ExtractLangTagFromUrlHelper_Relative("/zh/account"                      , "zh"                      , "/account");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK/account"                   , "zh-HK"                   , "/account");
            ExtractLangTagFromUrlHelper_Relative("/zh-123/account"                  , "zh-123"                  , "/account");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans/account"                 , "zh-Hans"                 , "/account");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK/account"              , "zh-Hans-HK"              , "/account");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123/account"             , "zh-Hans-123"             , "/account");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD/account"      , "zh-Hans-123-x-ABCD"      , "/account");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123/account", "zh-Hans-123-x-ABCDEFG123", "/account");

            ExtractLangTagFromUrlHelper_Relative("/zh/account?qs"                      , "zh"                      , "/account?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK/account?qs"                   , "zh-HK"                   , "/account?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-123/account?qs"                  , "zh-123"                  , "/account?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans/account?qs"                 , "zh-Hans"                 , "/account?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK/account?qs"              , "zh-Hans-HK"              , "/account?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123/account?qs"             , "zh-Hans-123"             , "/account?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD/account?qs"      , "zh-Hans-123-x-ABCD"      , "/account?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123/account?qs", "zh-Hans-123-x-ABCDEFG123", "/account?qs");

            ExtractLangTagFromUrlHelper_Relative("/zh/account/"                      , "zh"                      , "/account/");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK/account/"                   , "zh-HK"                   , "/account/");
            ExtractLangTagFromUrlHelper_Relative("/zh-123/account/"                  , "zh-123"                  , "/account/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans/account/"                 , "zh-Hans"                 , "/account/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK/account/"              , "zh-Hans-HK"              , "/account/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123/account/"             , "zh-Hans-123"             , "/account/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD/account/"      , "zh-Hans-123-x-ABCD"      , "/account/");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123/account/", "zh-Hans-123-x-ABCDEFG123", "/account/");

            ExtractLangTagFromUrlHelper_Relative("/zh/account/?qs"                      , "zh"                      , "/account/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK/account/?qs"                   , "zh-HK"                   , "/account/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-123/account/?qs"                  , "zh-123"                  , "/account/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans/account/?qs"                 , "zh-Hans"                 , "/account/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK/account/?qs"              , "zh-Hans-HK"              , "/account/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123/account/?qs"             , "zh-Hans-123"             , "/account/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD/account/?qs"      , "zh-Hans-123-x-ABCD"      , "/account/?qs");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123/account/?qs", "zh-Hans-123-x-ABCDEFG123", "/account/?qs");

            ExtractLangTagFromUrlHelper_Relative("/zh/account/x"                      , "zh"                      , "/account/x");
            ExtractLangTagFromUrlHelper_Relative("/zh-HK/account/x"                   , "zh-HK"                   , "/account/x");
            ExtractLangTagFromUrlHelper_Relative("/zh-123/account/x"                  , "zh-123"                  , "/account/x");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans/account/x"                 , "zh-Hans"                 , "/account/x");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK/account/x"              , "zh-Hans-HK"              , "/account/x");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123/account/x"             , "zh-Hans-123"             , "/account/x");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCD/account/x"      , "zh-Hans-123-x-ABCD"      , "/account/x");
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-123-x-ABCDEFG123/account/x", "zh-Hans-123-x-ABCDEFG123", "/account/x");

            ExtractLangTagFromUrlHelper_Relative("/azh"                      , "azh"                      , "/");
            ExtractLangTagFromUrlHelper_Relative("/azh-HK"                   , "azh-HK"                   , "/");
            ExtractLangTagFromUrlHelper_Relative("/azh-123"                  , "azh-123"                  , "/");
            ExtractLangTagFromUrlHelper_Relative("/azh-Hans"                 , "azh-Hans"                 , "/");
            ExtractLangTagFromUrlHelper_Relative("/azh-Hans-HK"              , "azh-Hans-HK"              , "/");
            ExtractLangTagFromUrlHelper_Relative("/azh-Hans-123"             , "azh-Hans-123"             , "/");
            ExtractLangTagFromUrlHelper_Relative("/azh-Hans-123-x-ABCD"      , "azh-Hans-123-x-ABCD"      , "/");
            ExtractLangTagFromUrlHelper_Relative("/azh-Hans-123-x-ABCDEFG123", "azh-Hans-123-x-ABCDEFG123", "/");


            ExtractLangTagFromUrlHelper_Relative("/zh-a"                     , null); // 1-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-aHK"                   , null); // 3-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-a123"                  , null); // 4-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-aaHans"                , null); // 6-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-aaHans-HK"             , null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-aaHans-123"            , null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-aaHans-123-x-ABCD"     , null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-aaHans-HK-x-ABCDEFG123", null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK-x-ABC"         , null); // < 4-char Private use subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK-x-"            , null); // < 4-char Private use subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK-x"             , null); // < 4-char Private use subtag = bad
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK-ABC"           , null); // Invalid subtag
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-HK-"              , null); // Invalid subtag

            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-K"               , null); // Invalid Region
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-23"              , null);
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-aHK"             , null);
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-a123"            , null);
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-H23"             , null);
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-12K"             , null);
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-12"              , null);
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-12-x-ABCD"       , null);
            ExtractLangTagFromUrlHelper_Relative("/zh-Hans-12-x-ABCDEFG123" , null);
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
