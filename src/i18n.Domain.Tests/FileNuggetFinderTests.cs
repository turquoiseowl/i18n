using System.Collections.Generic;
using System.IO;
using System.Linq;
using i18n.Domain.Concrete;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace i18n.Domain.Tests
{
    [TestClass]
    public class FileNuggetFinderTests
    {
        [TestMethod]
        public void FileNuggetFinder_disable_references()
        {
            var settingService = new SettingService_Mock();
            settingService.SetSetting("i18n.WhiteList", "ReferencesTest.txt");
            {
                i18nSettings settings = new i18nSettings(settingService);
                FileNuggetFinder finder = new FileNuggetFinder(settings);
                var templates = finder.ParseAll();

                var item = templates.Values.First();

                Assert.AreEqual(5, item.References.Count());
            }
            {
                //Disabling references
                settingService.SetSetting("i18n.DisableReferences", "true");
                i18nSettings settings = new i18nSettings(settingService);
                FileNuggetFinder finder = new FileNuggetFinder(settings);
                var templates = finder.ParseAll();

                var item = templates.Values.First();

                Assert.AreEqual(1, item.References.Count());
            }
        }

	[TestMethod]
        public void FileNuggetFinder_blacklist_wildcards()
        {
            var settingService = new SettingService_Mock();
            var root = Path.GetDirectoryName(settingService.GetConfigFileLocation());
            {
                //"*" wildcard
                settingService.SetSetting("i18n.BlackList", "./TestDir/*");
                i18nSettings settings = new i18nSettings(settingService);
                //FileNuggetFinder finder = new FileNuggetFinder(settings);
                List<string> expected = new List<string>()
                {
                    Path.Combine(root, "TestDir", "Dir02"),
                    Path.Combine(root, "TestDir", "Dir1"),
                };

                CollectionAssert.AreEqual(expected, settings.BlackList.ToList());
            }
            {
                //"?" wildcard
                settingService.SetSetting("i18n.BlackList", "./TestDir/Dir?");
                i18nSettings settings = new i18nSettings(settingService);
                List<string> expected = new List<string>()
                {
                    Path.Combine(root, "TestDir", "Dir1"),
                };

                CollectionAssert.AreEqual(expected, settings.BlackList.ToList());
            }
        }
    }
}
