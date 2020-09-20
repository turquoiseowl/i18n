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
    }
}
