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
    public class ResponseFilterTests
    {
        void Helper_ResponseFilter_can_patch_html_urls(string suffix, string pre, string expectedPatched, string requestUrl = "http://example.com/blog")
        {
            HttpRequestBase fakeRequest   = Substitute.For<HttpRequestBase>();
            HttpResponseBase fakeResponse = Substitute.For<HttpResponseBase>();
            HttpContextBase fakeContext   = Substitute.For<HttpContextBase>();

            fakeRequest.Url.Returns(requestUrl.IsSet() ? new Uri(requestUrl) : null);
            fakeResponse.Headers.Returns(new System.Net.WebHeaderCollection
            {
                //{ "Authorization", "xyz" }
            });

            fakeContext.Request.Returns(fakeRequest);
            fakeContext.Response.Returns(fakeResponse);

            i18n.EarlyUrlLocalizer obj = new i18n.EarlyUrlLocalizer(new UrlLocalizer());
            string post = obj.ProcessOutgoingNuggets(pre, suffix, fakeContext);
            Assert.AreEqual(expectedPatched, post);
        }

        [TestMethod]
        public void ResponseFilter_can_patch_html_urls()
        {
            // Non-rooted path as href/src url. This should become rooted based on the path of the current request url.
            // See impl. details in EarlyUrlLocalizer.LocalizeUrl. Reference issue #286.
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"123\"></img>"                                , "<img src=\"/fr/123\"></img>"                                , "http://example.com/Default.aspx");
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"123\"></img>"                                , "<img src=\"/fr/blogs/123\"></img>"                          , "http://example.com/blogs/Default.aspx");
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"123\"></img>"                                , "<img src=\"/fr/blogs/123\"></img>"                          , "http://example.com/blogs/");
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"123\"></img>"                                , "<img src=\"/fr/123\"></img>"                                , "http://example.com/blogs");
            // NB: for the following we use .txt rather than .jpg because the defaule outgoing URL filter excludes .jpg urls.
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"content/fred.txt\"></img>"                   , "<img src=\"/fr/content/fred.txt\"></img>"                   , "http://example.com/blog");
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"content/fred.txt\"></img>"                   , "<img src=\"/fr/blog/content/fred.txt\"></img>"              , "http://example.com/blog/");
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"/content/fred.txt\"></img>"                  , "<img src=\"/fr/content/fred.txt\"></img>"                   , "http://example.com/blog/");
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"http://example.com/content/fred.txt\"></img>", "<img src=\"http://example.com/fr/content/fred.txt\"></img>" , "http://example.com/blog/");
            Helper_ResponseFilter_can_patch_html_urls("fr", "<img src=\"http://other.com/content/fred.txt\"></img>"  , "<img src=\"http://other.com/content/fred.txt\"></img>"      , "http://example.com/blog/"); // NB: foreign site so no langtag added

            // One attribute - empty url
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"\"></a>",
                "<a href=\"\"></a>");

            // One attribute.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/\"></a>",
                "<a href=\"/fr\"></a>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/shop\"></a>",
                "<a href=\"/fr/shop\"></a>");

            // Two attributes.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/\" title=\"Home\"></a>",
                "<a href=\"/fr\" title=\"Home\"></a>");

            // Three attributes.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" href=\"/\" title=\"Home\"></a>",
                "<a class=\"corporate_logo\" href=\"/fr\" title=\"Home\"></a>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" href=\"/aaaa\" title=\"Home\"></a>",
                "<a class=\"corporate_logo\" href=\"/fr/aaaa\" title=\"Home\"></a>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" title=\"Home\" href=\"/\"></a>",
                "<a class=\"corporate_logo\" title=\"Home\" href=\"/fr\"></a>");

            // Nonrelevant tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script1 src=\"123\"></script1>",
                "<script1 src=\"123\"></script1>");

            // script tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");

            // img tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"></img>",
                "<img src=\"/fr/123\"></img>");

            // a tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"123\"></a>",
                "<a href=\"/fr/123\"></a>");

            // form tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<form action=\"123\"></form>",
                "<form action=\"/fr/123\"></form>");

            // Embedded tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"><a href=\"123\"></a></img>",
                "<img src=\"/fr/123\"><a href=\"/fr/123\"></a></img>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"><a href=\"123\" /></img>",
                "<img src=\"/fr/123\"><a href=\"/fr/123\" /></img>");

            // Different langtags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr-FR",
                "<script src=\"123\"></script>",
                "<script src=\"/fr-FR/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "zh-Hans",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "zh-Hans-HK",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans-HK/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "zh-Hans-HK-x-sadadssad-asdasdadad-asdsadad",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans-HK-x-sadadssad-asdasdadad-asdsadad/123\"></script>");

            // Relative and absolute URLs.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"/123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"http://example.com/123\"></script>",
                "<script src=\"http://example.com/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"https://example.com/123\"></script>",
                "<script src=\"https://example.com/fr/123\"></script>");

            // More complex paths.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d\"></script>",
                "<script src=\"/fr/123/a/b/c/d\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d.js\"></script>",
                "<script src=\"/fr/123/a/b/c/d.js\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d.X.Y.Z.js\"></script>",
                "<script src=\"/fr/123/a/b/c/d.X.Y.Z.js\"></script>");

            // Query strings.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b\"></script>",
                "<script src=\"/fr/123?a=b\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b&c=d\"></script>",
                "<script src=\"/fr/123?a=b&c=d\"></script>");

            // Fragments.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/123#foo\"></a>",
                "<a href=\"/fr/123#foo\"></a>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"123#foo\"></a>", // unrooted
                "<a href=\"/fr/123#foo\"></a>");

            // Query strings and fragments.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b#foo\"></script>",
                "<script src=\"/fr/123?a=b#foo\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b&c=d#foo\"></script>",
                "<script src=\"/fr/123?a=b&c=d#foo\"></script>");

            // Single full script tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123 \"></script>",
                "<script src=\"/fr/123 \"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123\"></script>",
                "<script src=\" /fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \"></script>",
                "<script src=\" /fr/123 \"></script>");

            // Two full script tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script><script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script><script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123 \"></script><script src=\"123 \"></script>",
                "<script src=\"/fr/123 \"></script><script src=\"/fr/123 \"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123\"></script><script src=\" 123\"></script>",
                "<script src=\" /fr/123\"></script><script src=\" /fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \"></script><script src=\" 123 \"></script>",
                "<script src=\" /fr/123 \"></script><script src=\" /fr/123 \"></script>");

            // Single short script tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />");

            // Two short script tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /><script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /><script src=\" /fr/123 \" />");

            // Two short script tags separated.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />        <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />        <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> <<<<       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> <<<<       <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> ><><>><<<><><><       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> ><><>><<<><><><       <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> <m> <n> </n> </m>       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> <m> <n> </n> </m>       <script src=\" /fr/123 \" />");

            // Script tags embedded in other tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body>",
                "<body>\n<script src=\" /fr/123 \" /><script src=\" /fr/123 \" /></body>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<body><body><body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body></body></body>",
                "<body><body><body>\n<script src=\" /fr/123 \" /><script src=\" /fr/123 \" /></body></body></body>");

            // Random spaces.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src =\"123\"></script>",
                "<script src =\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src     =\"123\"></script>",
                "<script src     =\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src= \"123\"></script>",
                "<script src= \"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=   \"123\"></script>",
                "<script src=   \"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src = \"123\"></script>",
                "<script src = \"/fr/123\"></script>");

            // Random linefeeds.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script\n>",
                "<script\nsrc\n=\"/fr/123\"></script\n>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script>",
                "<script\nsrc\n=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc=\n\"123\"></script>",
                "<script\nsrc=\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc=\n\n\n\"123\"></script>",
                "<script\nsrc=\n\n\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "\n<script\nsrc\n=\n\"123\"\n>\n</script\n>\n",
                "\n<script\nsrc\n=\n\"/fr/123\"\n>\n</script\n>\n");

            // Random CRLFs and tabs.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script\r\n>",
                "<script\r\nsrc\r\n=\"/fr/123\"></script\r\n>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script>",
                "<script\r\nsrc\r\n=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc=\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc=\r\n\r\n\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\r\n\r\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "\r\n<script\r\nsrc\r\n=\r\n\"123\"\r\n>\r\n</script\r\n>\r\n",
                "\r\n<script\r\nsrc\r\n=\r\n\"/fr/123\"\r\n>\r\n</script\r\n>\r\n");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"/fr/123\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t");

            // IGNORE_LOCALIZATION URLs.
            // These should not be changed by the filter.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}http://example.com/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"http://example.com/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}https://example.com/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"https://example.com/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}https://example.com/fr/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"https://example.com/fr/123\"></script>");


            string strMultilineScriptWithHref = @"<script>
  try {
    this._baseHref="""";
  }
  catch (e) {
  }
</script>
<div class=""page_style_a"">";

            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                strMultilineScriptWithHref,
                strMultilineScriptWithHref);

        }
    }
}
