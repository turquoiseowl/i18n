using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace i18n
{
    /// <summary>
    /// A post-build task for building a localization message database using GNU xgettext
    /// <see href="http://gnuwin32.sourceforge.net/packages/gettext.htm" />
    /// </summary>
    public class PostBuildTask
    {
        ///<summary>
        /// Runs GNU xgettext to extract a messages template file
        ///</summary>
        ///<param name="path"></param>
        public void Execute(string path)
        {
            var manifest = BuildProjectFileManifest(path);

            CreateMessageTemplate(path, manifest);

            MergeTemplateWithExistingLocales(path);

            File.Delete(manifest);
        }

        private static void MergeTemplateWithExistingLocales(string path)
        {
            var locales = Directory.GetDirectories(string.Format("{0}\\locale\\", path));
            var template = string.Format("{0}\\locale\\messages.pot", path);

            foreach (var messages in locales.Select(locale => string.Format("{0}\\messages.po", locale)))
            {
                if(File.Exists(messages))
                {
                    // http://www.gnu.org/s/hello/manual/gettext/msgmerge-Invocation.html
                    var args = string.Format("-U \"{0}\" \"{1}\"", messages, template);
                    RunWithOutput("gettext\\msgmerge.exe", args);
                }
                else
                {
                    File.Copy(template, messages);
                }
            }
        }

        private static void CreateMessageTemplate(string path, string manifest)
        {
            // http://www.gnu.org/s/hello/manual/gettext/xgettext-Invocation.html
            var args = string.Format("-LC# -k_ --omit-header --from-code=UTF-8 -o\"{0}\\locale\\messages.pot\" -f\"{1}\"", path, manifest);
            RunWithOutput("gettext\\xgettext.exe", args);
        }

        private static void RunWithOutput(string filename, string args)
        {
            var info = new ProcessStartInfo(filename, args)
            {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Console.WriteLine("{0} {1}", info.FileName, info.Arguments);
            var process = Process.Start(info);
            while (!process.StandardError.EndOfStream)
            {
                var line = process.StandardError.ReadLine();
                if (line == null)
                {
                    continue;
                }
                Console.WriteLine(line);
            }
        }

        private static string BuildProjectFileManifest(string path)
        {
            var cs = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            var razor = Directory.GetFiles(path, "*.cshtml", SearchOption.AllDirectories);
            var files = (new[] {cs, razor}).SelectMany(f => f).ToList();
            var temp = Path.GetTempFileName();
            using(var sw = File.CreateText(temp))
            {
                foreach(var file in files)
                {			
                    sw.WriteLine(file);
                }
            }
            return temp;
        }
    }
}