using i18n.Domain.Entities;
using i18n.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace i18n.Domain.Tests
{
    [TestClass]
    public class ReferenceContextTests
    {
        private const string Path = @"C:\Some\Path\File.ext";
        private const string Content =
        //   0          1          2           3         4           5
        //   0123456789 0 12345678901 2 345678901234567890 1 234567890123456789
            "The quick\r\n brown fox\r\n jumps over the lazy \r\ndog";

        [TestMethod]
        public void Create_PositionTooSmall_ReturnsFirstLine()
        {
            AssertExpectedContext(-10, CreateExpected("The quick", 1), "Position too small");
        }

        [TestMethod]
        public void Create_WithOutOfRangePosition_ReturnsLastLineWithNoContext()
        {
            AssertExpectedContext(Content.Length + 10, CreateExpected(string.Empty, 4), "Position too big");
        }

        [TestMethod]
        public void Create_PositionInFirstLine_ReturnsFirstLine()
        {
            AssertExpectedContext(2, CreateExpected("The quick", 1), "Position in first line");
        }

        [TestMethod]
        public void Create_PositionInThirdLine_ReturnsThirdLine()
        {
            AssertExpectedContext(23, CreateExpected("jumps over the lazy", 3), "Position in 3rd line");
        }

        [TestMethod]
        public void Create_PositionInLastLine_ReturnsLastLine()
        {
            AssertExpectedContext(46, CreateExpected("dog", 4), "Position in last line");
        }

        private static ReferenceContext CreateExpected(string context, int lineNumber)
        {
            return new ReferenceContext
            {
                Path = Path,
                LineNumber = lineNumber,
                Context = context
            };
        }

        private static void AssertExpectedContext(int position, ReferenceContext expected, string reason)
        {
            var actual = ReferenceContext.Create(Path, Content, position);

            Assert.AreEqual(expected.Path, actual.Path, "Path: " + reason);
            Assert.AreEqual(expected.LineNumber, actual.LineNumber, "LineNumber: " + reason);
            Assert.AreEqual(expected.Context, actual.Context, "Context:" + reason);
        }
    }
}
