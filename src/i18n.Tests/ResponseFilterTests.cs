using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace i18n.Tests
{
    [TestFixture]
    public class ResponseFilterTests
    {
        [Test]
        public void FilterProcessEntity()
        {
            //string pre = "«««123»»» «««123»»»";
            string pre = "[[[123]]] [[[123]]]";
            string post = i18n.ResponseFilter.ProcessEntity(pre, null);
            Assert.AreEqual("test.message test.message", post);
        }
    }
}
