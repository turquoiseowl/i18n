using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i18n;

namespace i18n.Tests.Tests
{
    /// <summary>
    /// Methods for testing the LanguageItem class functionality.
    /// </summary>
    [TestClass]
    public class LanguageItemTests
    {
        [TestMethod]
        public void LanguageItem_ParseHttpLanguageHeader()
        {
            LanguageItem[] languageItems;
            
            languageItems = LanguageItem.ParseHttpLanguageHeader("de;q=0.5");
            Assert.AreEqual(2, languageItems.Length);
            Assert.AreEqual(null, languageItems[0].LanguageTag);
            Assert.AreEqual(   2, languageItems[0].Quality);
            Assert.AreEqual(   0, languageItems[0].Ordinal);
            Assert.AreEqual(   0, languageItems[0].UseCount);
            Assert.AreEqual("de", languageItems[1].LanguageTag.ToString());
            Assert.AreEqual( 0.5, languageItems[1].Quality);
            Assert.AreEqual(   1, languageItems[1].Ordinal);
            Assert.AreEqual(   0, languageItems[1].UseCount);

            languageItems = LanguageItem.ParseHttpLanguageHeader("de;q=0.5, en;q=1");
            Assert.AreEqual(3, languageItems.Length);
            Assert.AreEqual(null, languageItems[0].LanguageTag);
            Assert.AreEqual(   2, languageItems[0].Quality);
            Assert.AreEqual(   0, languageItems[0].Ordinal);
            Assert.AreEqual(   0, languageItems[0].UseCount);
            Assert.AreEqual("en", languageItems[1].LanguageTag.ToString());
            Assert.AreEqual(   1, languageItems[1].Quality);
            Assert.AreEqual(   2, languageItems[1].Ordinal);
            Assert.AreEqual(   0, languageItems[1].UseCount);
            Assert.AreEqual("de", languageItems[2].LanguageTag.ToString());
            Assert.AreEqual( 0.5, languageItems[2].Quality);
            Assert.AreEqual(   1, languageItems[2].Ordinal);
            Assert.AreEqual(   0, languageItems[2].UseCount);

            languageItems = LanguageItem.ParseHttpLanguageHeader("de;q=0.5, en;q=1, fr-FR;q=0");
            Assert.AreEqual(4, languageItems.Length);
            Assert.AreEqual(null, languageItems[0].LanguageTag);
            Assert.AreEqual(   2, languageItems[0].Quality);
            Assert.AreEqual(   0, languageItems[0].Ordinal);
            Assert.AreEqual(   0, languageItems[0].UseCount);
            Assert.AreEqual("en", languageItems[1].LanguageTag.ToString());
            Assert.AreEqual(   1, languageItems[1].Quality);
            Assert.AreEqual(   2, languageItems[1].Ordinal);
            Assert.AreEqual(   0, languageItems[1].UseCount);
            Assert.AreEqual("de", languageItems[2].LanguageTag.ToString());
            Assert.AreEqual( 0.5, languageItems[2].Quality);
            Assert.AreEqual(   1, languageItems[2].Ordinal);
            Assert.AreEqual(   0, languageItems[2].UseCount);
            Assert.AreEqual("fr-FR", languageItems[3].LanguageTag.ToString());
            Assert.AreEqual(   0, languageItems[3].Quality);
            Assert.AreEqual(   3, languageItems[3].Ordinal);
            Assert.AreEqual(   0, languageItems[3].UseCount);

            languageItems = LanguageItem.ParseHttpLanguageHeader("de;q=0.5, en;q=1, fr-FR;q=0,ga;q=0.5");
            Assert.AreEqual(5, languageItems.Length);
            Assert.AreEqual(null, languageItems[0].LanguageTag);
            Assert.AreEqual(   2, languageItems[0].Quality);
            Assert.AreEqual(   0, languageItems[0].Ordinal);
            Assert.AreEqual(   0, languageItems[0].UseCount);
            Assert.AreEqual("en", languageItems[1].LanguageTag.ToString());
            Assert.AreEqual(   1, languageItems[1].Quality);
            Assert.AreEqual(   2, languageItems[1].Ordinal);
            Assert.AreEqual(   0, languageItems[1].UseCount);
            Assert.AreEqual("de", languageItems[2].LanguageTag.ToString());
            Assert.AreEqual( 0.5, languageItems[2].Quality);
            Assert.AreEqual(   1, languageItems[2].Ordinal);
            Assert.AreEqual(   0, languageItems[2].UseCount);
            Assert.AreEqual("ga", languageItems[3].LanguageTag.ToString());
            Assert.AreEqual( 0.5, languageItems[3].Quality);
            Assert.AreEqual(   4, languageItems[3].Ordinal);
            Assert.AreEqual(   0, languageItems[3].UseCount);
            Assert.AreEqual("fr-FR", languageItems[4].LanguageTag.ToString());
            Assert.AreEqual(   0, languageItems[4].Quality);
            Assert.AreEqual(   3, languageItems[4].Ordinal);
            Assert.AreEqual(   0, languageItems[4].UseCount);

            languageItems = LanguageItem.ParseHttpLanguageHeader("de;q=0.5, en;q=1, fr-FR;q=0,ga;q=0.5", new LanguageTag("en-CA"));
            Assert.AreEqual(5, languageItems.Length);
            Assert.AreEqual("en-CA", languageItems[0].LanguageTag.ToString());
            Assert.AreEqual(   2, languageItems[0].Quality);
            Assert.AreEqual(   0, languageItems[0].Ordinal);
            Assert.AreEqual(   0, languageItems[0].UseCount);
            Assert.AreEqual("en", languageItems[1].LanguageTag.ToString());
            Assert.AreEqual(   1, languageItems[1].Quality);
            Assert.AreEqual(   2, languageItems[1].Ordinal);
            Assert.AreEqual(   0, languageItems[1].UseCount);
            Assert.AreEqual("de", languageItems[2].LanguageTag.ToString());
            Assert.AreEqual( 0.5, languageItems[2].Quality);
            Assert.AreEqual(   1, languageItems[2].Ordinal);
            Assert.AreEqual(   0, languageItems[2].UseCount);
            Assert.AreEqual("ga", languageItems[3].LanguageTag.ToString());
            Assert.AreEqual( 0.5, languageItems[3].Quality);
            Assert.AreEqual(   4, languageItems[3].Ordinal);
            Assert.AreEqual(   0, languageItems[3].UseCount);
            Assert.AreEqual("fr-FR", languageItems[4].LanguageTag.ToString());
            Assert.AreEqual(   0, languageItems[4].Quality);
            Assert.AreEqual(   3, languageItems[4].Ordinal);
            Assert.AreEqual(   0, languageItems[4].UseCount);
        }

        /// <summary>
        /// Test the LanguageItem class's support for encoding a LanguageItem[] value as a string.
        /// </summary>
        [TestMethod]
        public void LanguageItem_CompactString()
        {
            LanguageItem[] languageItems;
            string strCompactString;
            // PAL set.
            languageItems = LanguageItem.ParseHttpLanguageHeader("de;q=0.5, en;q=1, fr-FR;q=0,ga;q=0.5", new LanguageTag("en-CA"));
            for (int i = 0; i < 2; ++i) {
                Assert.AreEqual(5, languageItems.Length);
                Assert.AreEqual("en-CA", languageItems[0].LanguageTag.ToString());
                Assert.AreEqual(   2, languageItems[0].Quality);
                Assert.AreEqual(   0, languageItems[0].Ordinal);
                Assert.AreEqual(   0, languageItems[0].UseCount);
                Assert.AreEqual("en", languageItems[1].LanguageTag.ToString());
                Assert.AreEqual(   1, languageItems[1].Quality);
                Assert.AreEqual(   2, languageItems[1].Ordinal);
                Assert.AreEqual(   0, languageItems[1].UseCount);
                Assert.AreEqual("de", languageItems[2].LanguageTag.ToString());
                Assert.AreEqual( 0.5, languageItems[2].Quality);
                Assert.AreEqual(   1, languageItems[2].Ordinal);
                Assert.AreEqual(   0, languageItems[2].UseCount);
                Assert.AreEqual("ga", languageItems[3].LanguageTag.ToString());
                Assert.AreEqual( 0.5, languageItems[3].Quality);
                Assert.AreEqual(   4, languageItems[3].Ordinal);
                Assert.AreEqual(   0, languageItems[3].UseCount);
                Assert.AreEqual("fr-FR", languageItems[4].LanguageTag.ToString());
                Assert.AreEqual(   0, languageItems[4].Quality);
                Assert.AreEqual(   3, languageItems[4].Ordinal);
                Assert.AreEqual(   0, languageItems[4].UseCount);
                // To compact string.
                strCompactString = LanguageItem.DehydrateLanguageItemsToString(languageItems);
                Assert.AreEqual("en-CA;q=2,de;q=0.5,en;q=1,fr-FR;q=0,ga;q=0.5", strCompactString);
                // From compact string.
                languageItems = LanguageItem.HydrateLanguageItemsFromString(strCompactString);
            }
            // PAL not set.
            languageItems = LanguageItem.ParseHttpLanguageHeader("de;q=0.5, en;q=1, fr-FR;q=0,ga;q=0.5", null); // <--- null for the PAL langtag
            for (int i = 0; i < 2; ++i) {
                Assert.AreEqual(5, languageItems.Length);
                Assert.AreEqual(null, languageItems[0].LanguageTag);
                Assert.AreEqual(   2, languageItems[0].Quality);
                Assert.AreEqual(   0, languageItems[0].Ordinal);
                Assert.AreEqual(   0, languageItems[0].UseCount);
                Assert.AreEqual("en", languageItems[1].LanguageTag.ToString());
                Assert.AreEqual(   1, languageItems[1].Quality);
                Assert.AreEqual(   2, languageItems[1].Ordinal);
                Assert.AreEqual(   0, languageItems[1].UseCount);
                Assert.AreEqual("de", languageItems[2].LanguageTag.ToString());
                Assert.AreEqual( 0.5, languageItems[2].Quality);
                Assert.AreEqual(   1, languageItems[2].Ordinal);
                Assert.AreEqual(   0, languageItems[2].UseCount);
                Assert.AreEqual("ga", languageItems[3].LanguageTag.ToString());
                Assert.AreEqual( 0.5, languageItems[3].Quality);
                Assert.AreEqual(   4, languageItems[3].Ordinal);
                Assert.AreEqual(   0, languageItems[3].UseCount);
                Assert.AreEqual("fr-FR", languageItems[4].LanguageTag.ToString());
                Assert.AreEqual(   0, languageItems[4].Quality);
                Assert.AreEqual(   3, languageItems[4].Ordinal);
                Assert.AreEqual(   0, languageItems[4].UseCount);
                // To compact string.
                strCompactString = LanguageItem.DehydrateLanguageItemsToString(languageItems);
                Assert.AreEqual("?;q=2,de;q=0.5,en;q=1,fr-FR;q=0,ga;q=0.5", strCompactString); // <--- '?' for the PAL langtag
                // From compact string.
                languageItems = LanguageItem.HydrateLanguageItemsFromString(strCompactString);
            }
            // Hydration of null/empty language items string.
            // Should result in a single-item language item array representing a null PAL.
            languageItems = LanguageItem.HydrateLanguageItemsFromString(null);
            Assert.AreEqual(1, languageItems.Length);
            Assert.AreEqual(null, languageItems[0].LanguageTag);
            Assert.AreEqual(   2, languageItems[0].Quality);
            Assert.AreEqual(   0, languageItems[0].Ordinal);
            Assert.AreEqual(   0, languageItems[0].UseCount);
        }
    }
}
