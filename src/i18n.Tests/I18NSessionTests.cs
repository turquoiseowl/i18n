using System;
using System.Web;
using FakeItEasy;
using NUnit.Framework;

namespace i18n.Tests
{
    [TestFixture]
    public class I18NSessionTests
    {
        [Test]
        public void Can_handle_markup_in_text()
        {
            var context = A.Fake<HttpContextBase>();
            var session = new I18NSession();
            var text = session.GetText(context, "Take the <br />next step!");
            Console.WriteLine(text);
        }
    }
}
