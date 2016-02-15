using System;
using System.IO;
using i18n.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace i18n.Domain.Tests
{
    [TestClass]
    public class PathNormalizerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MakeRelativePath_InvalidPath_Throws()
        {
            var anchorPath = string.Empty;
            PathNormalizer.MakeRelativePath(anchorPath, null);
        }

        [TestMethod]
        public void MakeRelativePath_NullOrEmptyAnchorPath_ReturnsPath()
        {
            var path = @"C:\some\path\file.ext";

            AssertNormalizedPath(null, path, path, "Null anchor path");
            AssertNormalizedPath(string.Empty, path, path, "Empty anchor path");
        }

        private static void AssertNormalizedPath(string anchorPath, string path, string expected, string reason)
        {
            var actual = PathNormalizer.MakeRelativePath(anchorPath, path);

            Assert.AreEqual(expected, actual, reason);
        }

        [TestMethod]
        public void MakeRelativePath_SimpleRelativePath_ReturnsRelativePath()
        {
            var anchorPath = @"C:\Some\Path";
            var relativePath = @"Another\Nested\File.ext";
            var path = Path.Combine(anchorPath, relativePath);

            AssertNormalizedPath(anchorPath, path, relativePath, "Simple relative path");
        }

        [TestMethod]
        public void MakeRelativePath_NoCommonPortion_ReturnsPath()
        {
            var anchorPath = @"C:\Some\Path";
            var path = @"D:\Some\Other\Path";
            AssertNormalizedPath(anchorPath, path, path, "No common path portion");
        }

        [TestMethod]
        public void MakeRelativePath_SomeCommonPorths_ReturnsRelativePath()
        {
            var commonPath = @"C:\Some\Common\Path";
            var anchorPath = Path.Combine(commonPath, @"Anchor\Path");
            var distinctPath = @"Distinct\Path\File.ext";
            var path = Path.Combine(commonPath, distinctPath);
            var relativePath = Path.Combine(@"..\..", distinctPath);
            AssertNormalizedPath(anchorPath, path, relativePath, "Some common path components");
        }
    }
}
