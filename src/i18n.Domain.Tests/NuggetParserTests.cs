using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i18n.Helpers;

namespace i18n.Domain.Tests
{
    [TestClass]
    public class NuggetParserTests
    {

    #region Helpers

        private void ParseAndComp(string nuggetString, Nugget rhs, bool equal = true)
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("[[[", "]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
           // Act.
            Nugget nugget = nuggetParser.BreakdownNugget(nuggetString);
           // Assert.
            if (equal) {
                Assert.AreEqual(nugget, rhs); }
            else {
                Assert.AreNotEqual(nugget, rhs); }
        }

        private void CanParseEntity_CustomNuggetTokens_Act(string entity, NuggetParser nuggetParser)
        {
           // Act.
            int i = 0;
            nuggetParser.ParseString(entity, delegate(string nuggetString, int pos, Nugget nugget1, string entity1)
            {
                switch (i++) {
                    case 0: {
                        Assert.AreEqual(nugget1, new Nugget { MsgId = "hello", FormatItems = new string[] { "{0}" } });
                        break;
                    }
                    case 1: {
                        Assert.AreEqual(nugget1, new Nugget { MsgId = "there", FormatItems = new string[] { "{0}", "{1}" }, Comment = "comment comment comment" });
                        break;
                    }
                    default:
                        Assert.Fail();
                        break;
                }
                return null;
            });
        }

    #endregion

        [TestMethod]
        public void NuggetParser_CanBreakdownNugget()
        {
            ParseAndComp("[[[msgid]]]"                                            , new Nugget { MsgId = "msgid" });
            ParseAndComp("[[[msgid|||{0}]]]"                                      , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}" } });
            ParseAndComp("[[[msgid|||{0}|||{1}]]]"                                , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}", "{1}" } });
            ParseAndComp("[[[msgid|||{0}|||{1}]]]"                                , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}", "{0}" } }, false);
            ParseAndComp("[[[msgid|||{0}|||{1}]]]"                                , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{1}", "{0}" } }, false);
            ParseAndComp("[[[msgid|||{0}|||{1}|||{3}|||{4}|||{2}|||{5}]]]"        , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}", "{1}", "{3}", "{4}", "{2}", "{5}" } });
            ParseAndComp("[[[msgid|||{0}///cmt]]]"                                , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}" }, Comment = "cmt" });
            ParseAndComp("[[[msgid|||{0}/// cmt]]]"                               , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}" }, Comment = "cmt" }, false);
            ParseAndComp("[[[msgid|||{0}/// cmt]]]"                               , new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}" }, Comment = " cmt" });
            ParseAndComp("[[[msgid|||{0}|||{1}|||{3}|||{4}|||{2}|||{5}/// cmt ]]]", new Nugget { MsgId = "msgid", FormatItems = new string[] { "{0}", "{1}", "{3}", "{4}", "{2}", "{5}" }, Comment = " cmt " });
            ParseAndComp("[[[msgid/// cmt ]]]"                                    , new Nugget { MsgId = "msgid", Comment = " cmt " });
            ParseAndComp("[[[msgid// cmt ]]]"                                     , new Nugget { MsgId = "msgid", Comment = " cmt " }, false);
            ParseAndComp("[[[msgid// cmt ]]]"                                     , new Nugget { MsgId = "msgid// cmt ", Comment = " cmt " }, false);
            ParseAndComp("[[[msgid// cmt ]]]"                                     , new Nugget { MsgId = "msgid// cmt " });
            ParseAndComp("[[[msgid|| cmt ]]]"                                     , new Nugget { MsgId = "msgid|| cmt " });
            ParseAndComp("[[[msgid|||| cmt ]]]"                                   , new Nugget { MsgId = "msgid", FormatItems = new string[] { "| cmt " } });
        }

        [TestMethod]
        public void NuggetParser_CanParseEntity01()
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("[[[", "]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            string entity = "<p>[[[hello]]]</p><p>[[[there]]]</p>";
           // Act.
            nuggetParser.ParseString(entity, delegate(string nuggetString, int pos, Nugget nugget1, string entity1)
            {
                if (pos == 3) {
                    Assert.AreEqual(nugget1, new Nugget { MsgId = "hello" }); }
                else if (pos == 21) {
                    Assert.AreEqual(nugget1, new Nugget { MsgId = "there" }); }
                else {
                    Assert.Fail(); }
                return null;
            });
        }

        [TestMethod]
        public void NuggetParser_CanParseEntity02()
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("[[[", "]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            string entity = "<p>[[[hello|||{0}]]]</p><p>[[[there]]]</p>";
           // Act.
            nuggetParser.ParseString(entity, delegate(string nuggetString, int pos, Nugget nugget1, string entity1)
            {
                if (pos == 3) {
                    Assert.AreEqual(nugget1, new Nugget { MsgId = "hello", FormatItems = new string[] { "{0}" } }); }
                else if (pos == 27) {
                    Assert.AreEqual(nugget1, new Nugget { MsgId = "there" }); }
                else {
                    Assert.Fail(); }
                return null;
            });
        }

        [TestMethod]
        public void NuggetParser_CanParseEntity03()
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("[[[", "]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            string entity = "<p>[[[hello|||{0}]]]</p><p>[[[there|||{0}|||{1}///comment comment comment]]]</p>";
           // Act.
            nuggetParser.ParseString(entity, delegate(string nuggetString, int pos, Nugget nugget1, string entity1)
            {
                if (pos == 3) {
                    Assert.AreEqual(nugget1, new Nugget { MsgId = "hello", FormatItems = new string[] { "{0}" } }); }
                else if (pos == 27) {
                    Assert.AreEqual(nugget1, new Nugget { MsgId = "there", FormatItems = new string[] { "{0}", "{1}" }, Comment = "comment comment comment" }); }
                else {
                    Assert.Fail(); }
                return null;
            });
        }

        [TestMethod]
        public void NuggetParser_CanParseEntity_CustomNuggetTokens01()
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("[[[[", "]]]]]", "||", "//");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            string entity = "<p>[[[[hello||{0}]]]]]</p><p>[[[[there||{0}||{1}//comment comment comment]]]]]</p>";
           // Act.
            CanParseEntity_CustomNuggetTokens_Act(entity, nuggetParser);
        }

        [TestMethod]
        public void NuggetParser_CanParseEntity_CustomNuggetTokens02()
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("[[[:", ":]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            string entity = "<p>[[[:hello|||{0}:]]]</p><p>[[[:there|||{0}|||{1}///comment comment comment:]]]</p>";
           // Act.
            CanParseEntity_CustomNuggetTokens_Act(entity, nuggetParser);
        }

        [TestMethod]
        public void NuggetParser_CanParseEntity_CustomNuggetTokens03()
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("```", "'''", "###", "@@@");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            string entity = "<p>```hello###{0}'''</p><p>```there###{0}###{1}@@@comment comment comment'''</p>";
           // Act.
            CanParseEntity_CustomNuggetTokens_Act(entity, nuggetParser);
        }

        [TestMethod]
        public void NuggetParser_CanParseEntity_MultiLineNugget01()
        {
           // Arrange.
            NuggetTokens nuggetTokens = new NuggetTokens("[[[", "]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            string entity = "<p>[[[hello\r\n%0|||{0}]]]</p><p>[[[there]]]</p>";
           // Act.
            int i = 0;
            nuggetParser.ParseString(entity, delegate(string nuggetString, int pos, Nugget nugget1, string entity1)
            {
                switch (i++) {
                    case 0: {
                        Assert.AreEqual(nugget1, new Nugget { MsgId = "hello\r\n%0", FormatItems = new string[] { "{0}" } });
                        break;
                    }
                    case 1: {
                        Assert.AreEqual(nugget1, new Nugget { MsgId = "there" });
                        break;
                    }
                    default:
                        Assert.Fail();
                        break;
                }
                return null;
            });
        }

        [TestMethod]
        [Description("Issue #110: Parsing a nugget with empty parameter in Response should not leave delimiters intact.")]
        public void NuggetParser_ResponseMode_CanParseEntity_EmptyParam() {
            var nuggetTokens = new NuggetTokens("[[[", "]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.ResponseProcessing);
            var input = "[[[Title: %0|||]]]";
            var result = nuggetParser.ParseString(input, (nuggetString, pos, nugget, i_entity) => {
                Assert.IsTrue(nugget.IsFormatted);
                var message = NuggetLocalizer.ConvertIdentifiersInMsgId(nugget.MsgId);
                message = String.Format(message, nugget.FormatItems);
                return message;
            });

            Assert.AreEqual("Title: ", result);
        }

        [TestMethod]
        [Description("Issue #110: Parsing a nugget with empty parameter in Source should leave delimiters intact.")]
        public void NuggetParser_SourceMode_CanParseEntity_EmptyParam() {
            var nuggetTokens = new NuggetTokens("[[[", "]]]", "|||", "///");
            NuggetParser nuggetParser = new NuggetParser(nuggetTokens, NuggetParser.Context.SourceProcessing);
            var input = "[[[Title: %0|||]]]";
            var result = nuggetParser.ParseString(input, (nuggetString, pos, nugget, i_entity) => {
                Assert.IsFalse(nugget.IsFormatted);
                return nugget.MsgId;
            });

            Assert.AreEqual("Title: %0|||", result);
        }
    }
}
