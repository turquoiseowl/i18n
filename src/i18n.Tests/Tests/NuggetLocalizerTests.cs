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
            ITextLocalizer textLocalizer = new TextLocalizer_Mockup();

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            string pre = "[[[123]]] [[[456]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("xxx123yyy xxx456yyy", post);
        }

        [TestMethod]
        public void NuggetLocalizer_can_process_nugget_multiline()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mockup();

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            string pre = "[[[1\r\n2]]] [[[\r\n3]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("xxx1\r\n2yyy xxx\r\n3yyy", post);
        }
    }
}
