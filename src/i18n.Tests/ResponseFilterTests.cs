using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace i18n.Tests
{
    [TestFixture]
    public class ResponseFilterTests
    {
        [Test]
        public void ResponseFilter_can_process_nuggets()
        {
            //string pre = "«««123»»» «««123»»»";
            string pre = "[[[123]]] [[[123]]]";
            string post = i18n.ResponseFilter.ProcessNuggets(null, pre);
            Assert.AreEqual("test.message test.message", post);
        }



        [Test]
        public void ResponseFilter_can_patch_html_urls()
        {
            // One attribute.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/\"></a>",
                "<a href=\"/fr\"></a>");

            // Two attributes.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/\" title=\"Home\"></a>",
                "<a href=\"/fr\" title=\"Home\"></a>");

            // Three attributes.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" href=\"/\" title=\"Home\"></a>",
                "<a class=\"corporate_logo\" href=\"/fr\" title=\"Home\"></a>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" href=\"/aaa\" title=\"Home\"></a>",
                "<a class=\"corporate_logo\" href=\"/fr/aaa\" title=\"Home\"></a>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" title=\"Home\" href=\"/\"></a>",
                "<a class=\"corporate_logo\" title=\"Home\" href=\"/fr\"></a>");

            // Nonrelevant tag.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script1 src=\"123\"></script1>",
                "<script1 src=\"123\"></script1>");

            // script tag.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");

            // img tag.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"></img>",
                "<img src=\"/fr/123\"></img>");

            // a tag.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"123\"></a>",
                "<a href=\"/fr/123\"></a>");

            // form tag.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<form action=\"123\"></form>",
                "<form action=\"/fr/123\"></form>");

            // Embedded tags.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"><a href=\"123\"></a></img>",
                "<img src=\"/fr/123\"><a href=\"/fr/123\"></a></img>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"><a href=\"123\" /></img>",
                "<img src=\"/fr/123\"><a href=\"/fr/123\" /></img>");

            // Different langtags.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr-FR",
                "<script src=\"123\"></script>",
                "<script src=\"/fr-FR/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "zh-Hans",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "zh-Hans-HK",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans-HK/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "zh-Hans-HK-x-sadadssad-asdasdadad-asdsadad",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans-HK-x-sadadssad-asdasdadad-asdsadad/123\"></script>");

            // Relative and absolute URLs.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"/123\"></script>",
                "<script src=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"http://example.com/123\"></script>",
                "<script src=\"http://example.com/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"https://example.com/123\"></script>",
                "<script src=\"https://example.com/fr/123\"></script>");

            // More complex paths.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d\"></script>",
                "<script src=\"/fr/123/a/b/c/d\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d.js\"></script>",
                "<script src=\"/fr/123/a/b/c/d.js\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d.X.Y.Z.js\"></script>",
                "<script src=\"/fr/123/a/b/c/d.X.Y.Z.js\"></script>");

            // Query strings.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b\"></script>",
                "<script src=\"/fr/123?a=b\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b&c=d\"></script>",
                "<script src=\"/fr/123?a=b&c=d\"></script>");

            // Single full script tag.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123 \"></script>",
                "<script src=\"/fr/123 \"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123\"></script>",
                "<script src=\" /fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \"></script>",
                "<script src=\" /fr/123 \"></script>");

            // Two full script tags.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script><script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script><script src=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123 \"></script><script src=\"123 \"></script>",
                "<script src=\"/fr/123 \"></script><script src=\"/fr/123 \"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123\"></script><script src=\" 123\"></script>",
                "<script src=\" /fr/123\"></script><script src=\" /fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \"></script><script src=\" 123 \"></script>",
                "<script src=\" /fr/123 \"></script><script src=\" /fr/123 \"></script>");

            // Single short script tag.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />");

            // Two short script tags.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /><script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /><script src=\" /fr/123 \" />");

            // Two short script tags separated.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />        <script src=\" /fr/123 \" />");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />        <script src=\" /fr/123 \" />");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> <<<<       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> <<<<       <script src=\" /fr/123 \" />");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> ><><>><<<><><><       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> ><><>><<<><><><       <script src=\" /fr/123 \" />");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" /fr/123 \" />");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> <m> <n> </n> </m>       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> <m> <n> </n> </m>       <script src=\" /fr/123 \" />");

            // Script tags embedded in other tags.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body>",
                "<body>\n<script src=\" /fr/123 \" /><script src=\" /fr/123 \" /></body>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<body><body><body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body></body></body>",
                "<body><body><body>\n<script src=\" /fr/123 \" /><script src=\" /fr/123 \" /></body></body></body>");

            // Random spaces.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src =\"123\"></script>",
                "<script src =\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src     =\"123\"></script>",
                "<script src     =\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src= \"123\"></script>",
                "<script src= \"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=   \"123\"></script>",
                "<script src=   \"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src = \"123\"></script>",
                "<script src = \"/fr/123\"></script>");

            // Random linefeeds.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script\n>",
                "<script\nsrc\n=\"/fr/123\"></script\n>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script>",
                "<script\nsrc\n=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc=\n\"123\"></script>",
                "<script\nsrc=\n\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc=\n\n\n\"123\"></script>",
                "<script\nsrc=\n\n\n\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "\n<script\nsrc\n=\n\"123\"\n>\n</script\n>\n",
                "\n<script\nsrc\n=\n\"/fr/123\"\n>\n</script\n>\n");

            // Random CRLFs and tabs.
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script\r\n>",
                "<script\r\nsrc\r\n=\"/fr/123\"></script\r\n>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script>",
                "<script\r\nsrc\r\n=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc=\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc=\r\n\r\n\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\r\n\r\n\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "\r\n<script\r\nsrc\r\n=\r\n\"123\"\r\n>\r\n</script\r\n>\r\n",
                "\r\n<script\r\nsrc\r\n=\r\n\"/fr/123\"\r\n>\r\n</script\r\n>\r\n");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"/fr/123\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"></script>");
            ResponseFilter_can_patch_html_urls(
                "fr",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t");


        }
        void ResponseFilter_can_patch_html_urls(string suffix, string pre, string expectedPatched)
        {
            string post = i18n.ResponseFilter.PatchHtmlUrls(null, pre, suffix, new UrlLocalizer());
            Assert.AreEqual(expectedPatched, post);
        }
    }
}
