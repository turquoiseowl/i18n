using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace i18n.Tests
{
    [TestFixture]
    public class PostBuildTaskTests
    {
        [Test]
        public void Can_process_message_template()
        {
            var task = new PostBuildTask(new PostBuildTaskConfiguration
                                             {
                                                 GetTextExecutable = @"gettext\xgettext.exe",
                                                 MsgMergeExecutable = @"gettext\msgmerge.exe",
                                                 InputPaths = new string[]{},
                                                 FileExtensions = new string[]{},
                                                 OutputPath = Path.GetTempPath(),
                                                 LocaleDirectoryName = "locale",
                                                 OutputFileNameWithoutPrefix = "messages",
                                                 TranslationFunctions = new []{"a","b"},
                                                 ProgramLanguage = "C#",
                                                 DryRun = "true",
                                                 Encoding = "UTF-8"
                                             });

            var writer = new StringWriter();
            Console.SetOut(writer);
            task.Execute();
            
            const string expected = @"^gettext\\xgettext.exe -LC# -ka -kb --omit-header --from-code=UTF-8 -o"".+"" -f"".+""\s*$";
            Assert.True(Regex.IsMatch(writer.ToString(), expected));
        }
    }
}
