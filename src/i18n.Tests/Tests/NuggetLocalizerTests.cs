using System;
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
        public void NuggetLocalizer_can_process_nugget_htmlencoded()
        {
            string pre;
            string post;
           // Repo stores un-HtmlEncoded msgid.
            {
               // Arrange.
                ITextLocalizer textLocalizer = new TextLocalizer_Mock_SingleMessage("foo&bar", "blahblah");
                i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService()), textLocalizer);
               // Lookup HtmlEncoded msgid.
                pre = "[[[foo&amp;bar]]]";
                post = obj.ProcessNuggets(pre, languages);
                Assert.AreEqual("blahblah", post);
               // Lookup un-HtmlEncoded msgid.
                pre = "[[[foo&bar]]]";
                post = obj.ProcessNuggets(pre, languages);
                Assert.AreEqual("blahblah", post);
            }
           // Repo stores HtmlEncoded msgid.
            {
               // Arrange.
                ITextLocalizer textLocalizer = new TextLocalizer_Mock_SingleMessage("foo&amp;bar", "blahblah");
                i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService()), textLocalizer);
               // Lookup HtmlEncoded msgid.
                pre = "[[[foo&amp;bar]]]";
                post = obj.ProcessNuggets(pre, languages);
                Assert.AreEqual("blahblah", post);
               // Lookup un-HtmlEncoded msgid.
                //pre = "[[[foo&bar]]]";
                //post = obj.ProcessNuggets(pre, languages);
                //Assert.AreEqual("blahblah", post);
                    // NB: this scenario is not supported at present.
                    // If it is deemed to be required, add an extra step to the delegate
                    // within NuggetLocalizer.ProcessNuggets such that HttpUtility.HtmlEncode(nugget.MsgId)
                    // is passed to GetText (and same for nugget.Comment).
            }
        }

        [TestMethod]
        public void NuggetLocalizer_can_process_nugget_singleline()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mock_PrefixSuffix("xxx", "yyy");

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService()), textLocalizer);

            string pre = "[[[123]]] [[[456]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("xxx123yyy xxx456yyy", post);
        }

        [TestMethod]
        public void NuggetLocalizer_can_process_nugget_multiline()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mock_PrefixSuffix("xxx", "yyy");

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService()), textLocalizer);

            string pre = "[[[1\r\n2]]] [[[\r\n3]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("xxx1\r\n2yyy xxx\r\n3yyy", post);
        }

        [TestMethod]
        [Description("Issue #165: Parsing a nugget with empty parameter in Response should not give format exception.")]
        public void NuggetLocalizer_can_process_formatted_nugget_with_two_variables_firstempty_secondnonempty()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mock_PrefixSuffix();

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService()), textLocalizer);

            string pre = "[[[Will occur %0 every %1 years||||||10///First variable is a month]]]";
                // Value for first variable is missing.
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("Will occur  every 10 years", post);
        }

        [TestMethod]
        [Description("Issue #165: Parsing a nugget with empty parameter in Response should not give format exception.")]
        public void NuggetLocalizer_can_process_formatted_nugget_with_two_variables_firstnonempty_secondempty()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mock_PrefixSuffix();

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService()), textLocalizer);

            string pre = "[[[Will occur %0 every %1 years|||April|||///First variable is a month]]]";
            // Value for second variable is missing.
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("Will occur April every  years", post);
        }


        [TestMethod]
        [Description("Issue #169: Translate parameter.")]
        public void NuggetLocalizer_can_translate_parameter()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mock_PrefixSuffix("!", "!");
            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            string pre = "[[[%0 is required|||(((ZipCode)))]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("!!ZipCode! is required!", post);
        }

        
        [TestMethod]
        public void NuggetLocalizer_can_visualize_nugget()
        {
            ITextLocalizer textLocalizer = new TextLocalizer_Mock_PrefixSuffix("xxx", "yyy");
            var settings = new i18nSettings(new WebConfigSettingService())
            {
                VisualizeMessages = true
            };

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(settings, textLocalizer);

            string pre = "[[[123]]] [[[456]]]";
            string post = obj.ProcessNuggets(pre, languages);
            Assert.AreEqual("!xxx123yyy! !xxx456yyy!", post);
        }

        [TestMethod]
        [Description("Can translate recursive parameters.")]
        public void NuggetLocalizer_can_translate_recursive_parameter()
        {
            var textLocalizer = new TextLocalizer_Mock_Generic();
            textLocalizer.AddMessage("pt", "Product", "Produto");
            textLocalizer.AddMessage("pt", "Order", "Pedido");
            textLocalizer.AddMessage("pt", "%0 Status", "Status do %0");
            textLocalizer.AddMessage("pt", "Current %0", "%0 atual");
            textLocalizer.AddMessage("pt", "Please select a %0", "Por favor escolha o %0");
            textLocalizer.AddMessage("pt", "The %0 has not been saved. Would you like to save the changes?", "O %0 não foi salvo. Deseja salvar as mudanças?");

            LanguageItem[] pt = LanguageItem.ParseHttpLanguageHeader("pt");

            i18n.NuggetLocalizer obj = new i18n.NuggetLocalizer(new i18nSettings(new WebConfigSettingService(null)), textLocalizer);

            Assert.AreEqual(obj.ProcessNuggets("[[[Please select a %0|||(((%0 Status|||(((Order))))))]]]", pt), "Por favor escolha o Status do Pedido");
            Assert.AreEqual(obj.ProcessNuggets("[[[Please select a %0|||(((%0 Status|||(((Product))))))]]]", pt), "Por favor escolha o Status do Produto");
            Assert.AreEqual(obj.ProcessNuggets("[[[The %0 has not been saved. Would you like to save the changes?|||(((Current %0|||(((Order))))))]]]", pt), "O Pedido atual não foi salvo. Deseja salvar as mudanças?");
        }


    }
}
