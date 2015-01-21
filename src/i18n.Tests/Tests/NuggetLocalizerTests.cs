using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i18n.Domain.Concrete;

namespace i18n.Tests
{
    [TestClass]
    public class NuggetLocalizerTests
    {
        LanguageItem[] languages = LanguageItem.ParseHttpLanguageHeader("en");

        [TestMethod]
        public void NuggetLocalizer_can_process_nugget_singleline()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mockup("xxx", "yyy");

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            string pre = "[[[123]]] [[[456]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("xxx123yyy xxx456yyy", post);
        }

        [TestMethod]
        public void NuggetLocalizer_can_process_nugget_multiline()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mockup("xxx", "yyy");

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            string pre = "[[[1\r\n2]]] [[[\r\n3]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("xxx1\r\n2yyy xxx\r\n3yyy", post);
        }

        [TestMethod]
        [Description("Issue #165: Parsing a nugget with empty parameter in Response should not give format exception.")]
        public void NuggetLocalizer_can_process_formatted_nugget_with_two_variables_firstempty_secondnonempty()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mockup();

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            string pre = "[[[Will occur %0 every %1 years||||||10///First variable is a month]]]";
                // Value for first variable is missing.
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("Will occur  every 10 years", post);
        }

        [TestMethod]
        [Description("Issue #165: Parsing a nugget with empty parameter in Response should not give format exception.")]
        public void NuggetLocalizer_can_process_formatted_nugget_with_two_variables_firstnonempty_secondempty()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mockup();

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            string pre = "[[[Will occur %0 every %1 years|||April|||///First variable is a month]]]";
                // Value for second variable is missing.
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("Will occur April every  years", post);
        }

        [TestMethod]
        public void NuggetLocalizer_can_visualize_nugget()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mockup("xxx", "yyy");
            var settings = new i18nSettings(new WebConfigSettingService(null))
            {
                VisualizeMessages = true
            };

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(settings, textLocalizer);

            string pre = "[[[123]]] [[[456]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("!xxx123yyy! !xxx456yyy!", post);
        }
    }
}
