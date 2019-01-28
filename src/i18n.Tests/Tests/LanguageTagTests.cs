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

            // #383
            ExtractLangTagFromUrlHelper("/zh?"                        , "zh"                      , "/?");
            ExtractLangTagFromUrlHelper("/zh?qs"                      , "zh"                      , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-HK?qs"                   , "zh-HK"                   , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-123?qs"                  , "zh-123"                  , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans?qs"                 , "zh-Hans"                 , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK?qs"              , "zh-Hans-HK"              , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123?qs"             , "zh-Hans-123"             , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD?qs"      , "zh-Hans-123-x-ABCD"      , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123?qs", "zh-Hans-123-x-ABCDEFG123", "/?qs");

            ExtractLangTagFromUrlHelper("/zh#"                        , "zh"                      , "/#");
            ExtractLangTagFromUrlHelper("/zh#bm"                      , "zh"                      , "/#bm");
            ExtractLangTagFromUrlHelper("/zh-HK#bm"                   , "zh-HK"                   , "/#bm");
            ExtractLangTagFromUrlHelper("/zh-123#bm"                  , "zh-123"                  , "/#bm");
            ExtractLangTagFromUrlHelper("/zh-Hans#bm"                 , "zh-Hans"                 , "/#bm");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK#bm"              , "zh-Hans-HK"              , "/#bm");
            ExtractLangTagFromUrlHelper("/zh-Hans-123#bm"             , "zh-Hans-123"             , "/#bm");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD#bm"      , "zh-Hans-123-x-ABCD"      , "/#bm");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123#bm", "zh-Hans-123-x-ABCDEFG123", "/#bm");

            ExtractLangTagFromUrlHelper("/zh/?qs"                      , "zh"                      , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-HK/?qs"                   , "zh-HK"                   , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-123/?qs"                  , "zh-123"                  , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans/?qs"                 , "zh-Hans"                 , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/?qs"              , "zh-Hans-HK"              , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/?qs"             , "zh-Hans-123"             , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/?qs"      , "zh-Hans-123-x-ABCD"      , "/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/?qs", "zh-Hans-123-x-ABCDEFG123", "/?qs");

            ExtractLangTagFromUrlHelper("/zh/account"                      , "zh"                      , "/account");
            ExtractLangTagFromUrlHelper("/zh-HK/account"                   , "zh-HK"                   , "/account");
            ExtractLangTagFromUrlHelper("/zh-123/account"                  , "zh-123"                  , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans/account"                 , "zh-Hans"                 , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account"              , "zh-Hans-HK"              , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account"             , "zh-Hans-123"             , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account"      , "zh-Hans-123-x-ABCD"      , "/account");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account", "zh-Hans-123-x-ABCDEFG123", "/account");

            ExtractLangTagFromUrlHelper("/zh/account?qs"                      , "zh"                      , "/account?qs");
            ExtractLangTagFromUrlHelper("/zh-HK/account?qs"                   , "zh-HK"                   , "/account?qs");
            ExtractLangTagFromUrlHelper("/zh-123/account?qs"                  , "zh-123"                  , "/account?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans/account?qs"                 , "zh-Hans"                 , "/account?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account?qs"              , "zh-Hans-HK"              , "/account?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account?qs"             , "zh-Hans-123"             , "/account?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account?qs"      , "zh-Hans-123-x-ABCD"      , "/account?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account?qs", "zh-Hans-123-x-ABCDEFG123", "/account?qs");

            ExtractLangTagFromUrlHelper("/zh/account/"                      , "zh"                      , "/account/");
            ExtractLangTagFromUrlHelper("/zh-HK/account/"                   , "zh-HK"                   , "/account/");
            ExtractLangTagFromUrlHelper("/zh-123/account/"                  , "zh-123"                  , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/"                 , "zh-Hans"                 , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/"              , "zh-Hans-HK"              , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/"             , "zh-Hans-123"             , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account/"      , "zh-Hans-123-x-ABCD"      , "/account/");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account/", "zh-Hans-123-x-ABCDEFG123", "/account/");

            ExtractLangTagFromUrlHelper("/zh/account/?qs"                      , "zh"                      , "/account/?qs");
            ExtractLangTagFromUrlHelper("/zh-HK/account/?qs"                   , "zh-HK"                   , "/account/?qs");
            ExtractLangTagFromUrlHelper("/zh-123/account/?qs"                  , "zh-123"                  , "/account/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/?qs"                 , "zh-Hans"                 , "/account/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/?qs"              , "zh-Hans-HK"              , "/account/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/?qs"             , "zh-Hans-123"             , "/account/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account/?qs"      , "zh-Hans-123-x-ABCD"      , "/account/?qs");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account/?qs", "zh-Hans-123-x-ABCDEFG123", "/account/?qs");

            ExtractLangTagFromUrlHelper("/zh/account/x"                      , "zh"                      , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-HK/account/x"                   , "zh-HK"                   , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-123/account/x"                  , "zh-123"                  , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans/account/x"                 , "zh-Hans"                 , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-HK/account/x"              , "zh-Hans-HK"              , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123/account/x"             , "zh-Hans-123"             , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCD/account/x"      , "zh-Hans-123-x-ABCD"      , "/account/x");
            ExtractLangTagFromUrlHelper("/zh-Hans-123-x-ABCDEFG123/account/x", "zh-Hans-123-x-ABCDEFG123", "/account/x");

            ExtractLangTagFromUrlHelper("/azh"                      , "azh"                      , "/");
            ExtractLangTagFromUrlHelper("/azh-HK"                   , "azh-HK"                   , "/");
            ExtractLangTagFromUrlHelper("/azh-123"                  , "azh-123"                  , "/");
            ExtractLangTagFromUrlHelper("/azh-Hans"                 , "azh-Hans"                 , "/");
            ExtractLangTagFromUrlHelper("/azh-Hans-HK"              , "azh-Hans-HK"              , "/");
            ExtractLangTagFromUrlHelper("/azh-Hans-123"             , "azh-Hans-123"             , "/");
            ExtractLangTagFromUrlHelper("/azh-Hans-123-x-ABCD"      , "azh-Hans-123-x-ABCD"      , "/");
            ExtractLangTagFromUrlHelper("/azh-Hans-123-x-ABCDEFG123", "azh-Hans-123-x-ABCDEFG123", "/");


            ExtractLangTagFromUrlHelper("/zh-a"                     , null); // 1-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper("/zh-aHK"                   , null); // 3-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper("/zh-a123"                  , null); // 4-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper("/zh-aaHans"                , null); // 6-char Script/Region subtag = bad
            ExtractLangTagFromUrlHelper("/zh-aaHans-HK"             , null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper("/zh-aaHans-123"            , null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper("/zh-aaHans-123-x-ABCD"     , null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper("/zh-aaHans-HK-x-ABCDEFG123", null); // 6-char Script subtag = bad
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x-ABC"         , null); // < 4-char Private use subtag = bad
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x-"            , null); // < 4-char Private use subtag = bad
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-x"             , null); // < 4-char Private use subtag = bad
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-ABC"           , null); // Invalid subtag
            ExtractLangTagFromUrlHelper("/zh-Hans-HK-"              , null); // Invalid subtag

            ExtractLangTagFromUrlHelper("/zh-Hans-K"               , null); // Invalid Region
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
