using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i18n;

namespace i18n.Tests.Tests
{
    /// <summary>
    /// Methods for testing of the Language Matching algorithm.
    /// </summary>
    [TestClass]
    public class LanguageMatchingTests
    {

        public const string AppLanguagesCsv = "de,de-at,en,en-gb,zh-cn,zh-tw";
        static readonly IEnumerable<KeyValuePair<string, LanguageTag> > AppLanguages = AppLanguagesCsv.Split(',').Select(x => new KeyValuePair<string, LanguageTag>(x, new LanguageTag(x)));

    #region Helpers

        void Helper_LanguageMatching(
            string expected,
            string userLanguages)
        {
            LanguageItem [] UserLanguages = userLanguages.Split(',').Select(x => new LanguageItem(new LanguageTag(x), 1, 0)).ToArray();
            string text;
            LanguageTag langtag = LanguageMatching.MatchLists(
                UserLanguages, 
                AppLanguages, 
                null, 
                null, 
                out text);
            Assert.AreEqual(expected, langtag != null ? langtag.ToString() : "");
        }

    #endregion

        [TestMethod]
		[TestCategory("Unit")]
        public void LanguageMatching_Basic()
        {
           //                       Result            UserLanguages                 Notes
           //                       -----------------------------------------------------------------------------------------------------------

            Helper_LanguageMatching(""              , "");
            Helper_LanguageMatching("de"            , "de");
            Helper_LanguageMatching("de"            , "de-ch");                     // de-ch not an AL, so its parent is selected
            Helper_LanguageMatching("de-at"         , "de-at");
            Helper_LanguageMatching("de-at"         , "de-at,de-ch");
            Helper_LanguageMatching("de-at"         , "de-ch,de-at");               // refinement to PAL Prioritization: de-at is chosen over de.
            Helper_LanguageMatching("de"            , "de,de");
            Helper_LanguageMatching("de"            , "de,de-ch");


            //TODO -- more tests.

        }

    }
}
