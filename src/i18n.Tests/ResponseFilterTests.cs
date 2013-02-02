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
            string post = i18n.ResponseFilter.ProcessNuggets(pre, null);
            Assert.AreEqual("test.message test.message", post);
        }



        [Test]
        public void ResponseFilter_can_patch_script_urls()
        {
            // Single full script tag.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\"123 \"></script>",
                "<script src=\"123?fr \"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123\"></script>",
                "<script src=\" 123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \"></script>",
                "<script src=\" 123?fr \"></script>");

            // Two full script tags.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\"123\"></script><script src=\"123\"></script>",
                "<script src=\"123?fr\"></script><script src=\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\"123 \"></script><script src=\"123 \"></script>",
                "<script src=\"123?fr \"></script><script src=\"123?fr \"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123\"></script><script src=\" 123\"></script>",
                "<script src=\" 123?fr\"></script><script src=\" 123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \"></script><script src=\" 123 \"></script>",
                "<script src=\" 123?fr \"></script><script src=\" 123?fr \"></script>");

            // Single short script tag.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" />",
                "<script src=\" 123?fr \" />");

            // Two short script tags.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" /><script src=\" 123 \" />",
                "<script src=\" 123?fr \" /><script src=\" 123?fr \" />");

            // Two short script tags separated.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" 123?fr \" />        <script src=\" 123?fr \" />");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" 123?fr \" />        <script src=\" 123?fr \" />");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" /> <<<<       <script src=\" 123 \" />",
                "<script src=\" 123?fr \" /> <<<<       <script src=\" 123?fr \" />");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" /> ><><>><<<><><><       <script src=\" 123 \" />",
                "<script src=\" 123?fr \" /> ><><>><<<><><><       <script src=\" 123?fr \" />");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" 123 \" />",
                "<script src=\" 123?fr \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" 123?fr \" />");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=\" 123 \" /> <m> <n> </n> </m>       <script src=\" 123 \" />",
                "<script src=\" 123?fr \" /> <m> <n> </n> </m>       <script src=\" 123?fr \" />");

            // Script tags embedded in other tags.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body>",
                "<body>\n<script src=\" 123?fr \" /><script src=\" 123?fr \" /></body>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<body><body><body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body></body></body>",
                "<body><body><body>\n<script src=\" 123?fr \" /><script src=\" 123?fr \" /></body></body></body>");

            // Random spaces.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src =\"123\"></script>",
                "<script src =\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src     =\"123\"></script>",
                "<script src     =\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src= \"123\"></script>",
                "<script src= \"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src=   \"123\"></script>",
                "<script src=   \"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script src = \"123\"></script>",
                "<script src = \"123?fr\"></script>");

            // Random linefeeds.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script\n>",
                "<script\nsrc\n=\"123?fr\"></script\n>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script>",
                "<script\nsrc\n=\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\nsrc=\n\"123\"></script>",
                "<script\nsrc=\n\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\nsrc=\n\n\n\"123\"></script>",
                "<script\nsrc=\n\n\n\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "\n<script\nsrc\n=\n\"123\"\n>\n</script\n>\n",
                "\n<script\nsrc\n=\n\"123?fr\"\n>\n</script\n>\n");

            // Random CRLFs and tabs.
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script\r\n>",
                "<script\r\nsrc\r\n=\"123?fr\"></script\r\n>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script>",
                "<script\r\nsrc\r\n=\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\nsrc=\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\nsrc=\r\n\r\n\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\r\n\r\n\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "\r\n<script\r\nsrc\r\n=\r\n\"123\"\r\n>\r\n</script\r\n>\r\n",
                "\r\n<script\r\nsrc\r\n=\r\n\"123?fr\"\r\n>\r\n</script\r\n>\r\n");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123?fr\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"123?fr\"></script>");
            ResponseFilter_can_patch_script_urls(
                "fr",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123?fr\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t");


        }
        void ResponseFilter_can_patch_script_urls(string suffix, string pre, string expectedPatched)
        {
            string post = i18n.ResponseFilter.PatchScriptUrls(pre, suffix);
            Assert.AreEqual(expectedPatched, post);
        }
    }
}
