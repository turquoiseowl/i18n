using System;
using System.Collections.Generic;
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
        ///<param name="gettext"> </param>
        ///<param name="msgmerge"> </param>
        ///<param name="inputPath"></param>
        public void Execute(string outputPath, string gettext = null, string msgmerge = null, string[] inputPaths = null)
        {
            if (inputPaths == null || inputPaths.Length == 0) inputPaths = new string[] { outputPath };

            var manifest = BuildProjectFileManifest(inputPaths);

            CreateMessageTemplate(outputPath, manifest, gettext);

            MergeTemplateWithExistingLocales(outputPath, msgmerge);

            File.Delete(manifest);
        }

        private static void MergeTemplateWithExistingLocales(string outputPath, string options)
        {
            var locales = Directory.GetDirectories(string.Format("{0}\\locale\\", outputPath));
            var template = string.Format("{0}\\locale\\messages.pot", outputPath);

            foreach (var messages in locales.Select(locale => string.Format("{0}\\messages.po", locale)))
            {
                if (File.Exists(messages))
                {
                    // http://www.gnu.org/s/hello/manual/gettext/msgmerge-Invocation.html
                    var args = string.Format("{2} -U \"{0}\" \"{1}\"", messages, template, options);
                    RunWithOutput("gettext\\msgmerge.exe", args);
                }
                else
                {
                    File.Copy(template, messages);
                }
            }
        }

        private static void CreateMessageTemplate(string outputPath, string manifest, string options)
        {
            // http://www.gnu.org/s/hello/manual/gettext/xgettext-Invocation.html
            var args = string.Format("{2} -LC# -k_ -k__ --from-code=UTF-8 -o\"{0}\\locale\\messages.pot\" -f\"{1}\"", outputPath, manifest, options);
            RunWithOutput("gettext\\xgettext.exe", args); // Mark H bodge
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

        private static string[] GetFiles(string[] paths, string pattern)
        {
            var files = new List<string>();

            foreach (var path in paths)
            {
                var directories = Directory.GetDirectories(path);

                foreach (var dir in directories.Where(x => !x.EndsWith("\\obj")))
                {
                    files.AddRange(Directory.GetFiles(dir, pattern, SearchOption.AllDirectories));
                }
            }

            return files.ToArray();
        }

        private static string BuildProjectFileManifest(string[] paths)
        {
            var cs = GetFiles(paths, "*.cs");
            var razor = GetFiles(paths, "*.cshtml");
            var files = (new[] { cs, razor }).SelectMany(f => f).ToList();

            var temp = Path.GetTempFileName();
            using (var sw = File.CreateText(temp))
            {
                foreach (var file in files)
                {
                    sw.WriteLine(file);
                }
            }
            return temp;
        }
    }
}