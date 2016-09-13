using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i18n.Domain.Concrete;
using i18n.Helpers;
using NSubstitute;

namespace i18n.Tests
{
    [TestClass]
    public class UrlLocalizerTests
    {
        void Helper_UrlLocalizer_FilterIncoming(string urlPath, bool expected_result = false)
        {
            UriBuilder ub = new UriBuilder("http://example.com");
            ub.Path = urlPath;
            UrlLocalizer obj = new UrlLocalizer();
            bool result = obj.FilterIncoming(ub.Uri);
            Assert.AreEqual(expected_result, result);
        }

        /// <summary>
        /// Effectively test the default UrlLocalizer.QuickUrlExclusionFilter setting.
        /// </summary>
        [TestMethod]
        public void UrlLocalizer_FilterIncoming()
        {
            Helper_UrlLocalizer_FilterIncoming("/api/blogs/12345");
            Helper_UrlLocalizer_FilterIncoming("/Api/blogs/12345");
            Helper_UrlLocalizer_FilterIncoming("/API/blogs/12345");
            Helper_UrlLocalizer_FilterIncoming("/apinine/12345", true);
            Helper_UrlLocalizer_FilterIncoming("/Apinine/12345", true);
            Helper_UrlLocalizer_FilterIncoming("/APININE/12345", true);

            Helper_UrlLocalizer_FilterIncoming("sitemap.xml");
            Helper_UrlLocalizer_FilterIncoming("sitemap.xml/123", true);

            Helper_UrlLocalizer_FilterIncoming("123.css");
            Helper_UrlLocalizer_FilterIncoming("123.csst", true);

            Helper_UrlLocalizer_FilterIncoming("123.less");
            Helper_UrlLocalizer_FilterIncoming("123.jpg");
            Helper_UrlLocalizer_FilterIncoming("123.jpeg");
            Helper_UrlLocalizer_FilterIncoming("123.png");
            Helper_UrlLocalizer_FilterIncoming("123.gif");
            Helper_UrlLocalizer_FilterIncoming("123.ico");
            Helper_UrlLocalizer_FilterIncoming("123.svg");
            Helper_UrlLocalizer_FilterIncoming("123.woff");
            Helper_UrlLocalizer_FilterIncoming("123.woff2");
            Helper_UrlLocalizer_FilterIncoming("123.ttf");
            Helper_UrlLocalizer_FilterIncoming("123.eot");
        }
    }
}
