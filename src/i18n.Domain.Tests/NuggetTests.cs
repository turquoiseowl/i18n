using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i18n.Helpers;
using System.Linq;

namespace i18n.Domain.Tests
{
    [TestClass]
    public class NuggetTests
    {
        private void CompareNugget(Nugget n1, Nugget n2, bool equal)
        {
           // Test equality.
            if (equal) {
                Assert.AreEqual(n1, n2);
            }
            else {
                Assert.AreNotEqual(n1, n2);
            }
           // Test hash code.
            int h1 = n1.GetHashCode();
            int h2 = n2.GetHashCode();
           // · If two objects are equal then they must have the same hash code.
            if (n1.Equals(n2)) {
                Assert.AreEqual(h1, h2);
            }
           // · If two objects have different hash codes then they must be unequal.
            if (h1 != h2) {
                Assert.AreNotEqual(n1, n2);
            }
        }

        /// <summary>
        /// Test the Nugget.Equals and Nugget.GetHashCode implementations.
        /// </summary>
        [TestMethod]
        public void Nugget_EqualsAndHashCode()
        {
            Nugget nugget01 = new Nugget { MsgId = "msgid" };
            Nugget nugget02 = new Nugget { MsgId = "msgid", FormatItems = new string[] { "fi1", "fi2" } };
            Nugget nugget03 = new Nugget { MsgId = "msgid", Comment = "comment" };
            Nugget nugget04 = new Nugget { MsgId = "msgid", FormatItems = new string[] { "fi1", "fi2" }, Comment = "comment" };
            Nugget nugget05 = new Nugget { MsgId = "msgid", FormatItems = new string[] { "fi1" } };
            Nugget nugget06 = new Nugget { MsgId = "msgid", FormatItems = new string[] {}, Comment = "comment" };

            Nugget nugget11 = new Nugget { MsgId = "msgid" };
            Nugget nugget12 = new Nugget { MsgId = "msgid", FormatItems = new string[] { "fi1", "fi2" } };
            Nugget nugget13 = new Nugget { MsgId = "msgid", Comment = "comment" };
            Nugget nugget14 = new Nugget { MsgId = "msgid", FormatItems = new string[] { "fi1", "fi2" }, Comment = "comment" };
            Nugget nugget15 = new Nugget { MsgId = "msgid", FormatItems = new string[] { "fi1" } };
            Nugget nugget16 = new Nugget { MsgId = "msgid", FormatItems = new string[] {}, Comment = "comment" };

            CompareNugget(nugget01, nugget01, true);
            CompareNugget(nugget02, nugget02, true);
            CompareNugget(nugget03, nugget03, true);
            CompareNugget(nugget04, nugget04, true);
            CompareNugget(nugget05, nugget05, true);
            CompareNugget(nugget06, nugget06, true);

            CompareNugget(nugget01, nugget11, true);
            CompareNugget(nugget02, nugget12, true);
            CompareNugget(nugget03, nugget13, true);
            CompareNugget(nugget04, nugget14, true);
            CompareNugget(nugget05, nugget15, true);
            CompareNugget(nugget06, nugget16, true);

            CompareNugget(nugget01, nugget11, true);
            CompareNugget(nugget02, nugget11, false);
            CompareNugget(nugget03, nugget11, false);
            CompareNugget(nugget04, nugget11, false);
            CompareNugget(nugget05, nugget11, false);
            CompareNugget(nugget06, nugget11, false);

            CompareNugget(nugget01, nugget12, false);
            CompareNugget(nugget02, nugget12, true);
            CompareNugget(nugget03, nugget12, false);
            CompareNugget(nugget04, nugget12, false);
            CompareNugget(nugget05, nugget12, false);
            CompareNugget(nugget06, nugget12, false);

            CompareNugget(nugget01, nugget13, false);
            CompareNugget(nugget02, nugget13, false);
            CompareNugget(nugget03, nugget13, true);
            CompareNugget(nugget04, nugget13, false);
            CompareNugget(nugget05, nugget13, false);
            CompareNugget(nugget06, nugget13, false);

            CompareNugget(nugget01, nugget14, false);
            CompareNugget(nugget02, nugget14, false);
            CompareNugget(nugget03, nugget14, false);
            CompareNugget(nugget04, nugget14, true);
            CompareNugget(nugget05, nugget14, false);
            CompareNugget(nugget06, nugget14, false);

            CompareNugget(nugget01, nugget15, false);
            CompareNugget(nugget02, nugget15, false);
            CompareNugget(nugget03, nugget15, false);
            CompareNugget(nugget04, nugget15, false);
            CompareNugget(nugget05, nugget15, true);
            CompareNugget(nugget06, nugget15, false);

            CompareNugget(nugget01, nugget16, false);
            CompareNugget(nugget02, nugget16, false);
            CompareNugget(nugget03, nugget16, false);
            CompareNugget(nugget04, nugget16, false);
            CompareNugget(nugget05, nugget16, false);
            CompareNugget(nugget06, nugget16, true);
        }

    }
}
