using NUnit.Framework;
using i18n.Tests.Helpers;

namespace i18n.Tests
{
    [TestFixture]
    public class SessionTests : MockSessionProvider
    {

        /// <summary>
        /// Reset session provider each test run.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            Initialize();
        }

        [Test]
        public void WorksWithMissingAcceptLanguageHeader()
        {
            // Act
            HttpContext.Request.Headers.Remove("Accept-Language");
            var text = _("WorksWithMissingAcceptLanguageHeader", HttpContext);

            // Assert
            Assert.AreEqual(text, "WorksWithMissingAcceptLanguageHeader");

        }

        [Test]
        public void WorksWithAServiceContext()
        {
            // Act
            var text = _("WorksWithAServiceContext", LocalizingService, new[] { DefaultLanguage });

            // Assert
            Assert.AreEqual(text, "WorksWithAServiceContext");
            Assert.AreEqual(DefaultLanguage, Session.GetLanguageFromSessionOrService(HttpContext));

        }

        [Test]
        public void WorksWithHttpContext()
        {
            // Act
            var text = _("WorksWithHttpContext", HttpContext);

            // Assert
            Assert.AreEqual(text, "WorksWithHttpContext");

        }

        [Test]
        public void WorksWithMarkupInHttpContext()
        {
            // Act
            var text = _("Take the <br />next step!", HttpContext);

            // Assert
            Assert.AreEqual(text, text);

        }

    }
}
