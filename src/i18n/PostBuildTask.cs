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
        private readonly PostBuildTaskConfiguration _config;

        public PostBuildTask(PostBuildTaskConfiguration config)
        {
            _config = config;
        }

        ///<summary>
        /// Runs GNU xgettext to extract a messages template file
        ///</summary>
        public void Execute()
        {
            var manifest = BuildProjectFileManifest();

            try
            {
                var localePath = Path.Combine(_config.OutputPath, _config.LocaleDirectoryName);

                if (!Directory.Exists(localePath))
                {
                    Directory.CreateDirectory(localePath);
                }

                CreateMessageTemplate(manifest);
                MergeTemplateWithExistingLocales();
            }
            finally
            {
                File.Delete(manifest);
            }
        }

        private void MergeTemplateWithExistingLocales()
        {
            var localePath = Path.Combine(_config.OutputPath, _config.LocaleDirectoryName);
            var localeFile = Path.Combine(localePath, _config.OutputFileNameWithoutPrefix + ".pot");

            var locales = Directory.GetDirectories(localePath);

            foreach (var messages in locales.Select(locale => Path.Combine(locale, _config.OutputFileNameWithoutPrefix + ".po")))
            {
                if (File.Exists(messages))
                {
                    // http://www.gnu.org/s/hello/manual/gettext/msgmerge-Invocation.html
                    var args = string.Format("-U \"{0}\" \"{1}\"", messages, localeFile);
                    RunWithOutput(_config.MsgMergeExecutable, args);
                }
                else
                {
                    File.Copy(localeFile, messages);
                }
            }
        }

        private void CreateMessageTemplate(string manifest)
        {
            var functions = string.Join(" -k", _config.TranslationFunctions);

            var outputFile = Path.Combine(_config.OutputPath, _config.LocaleDirectoryName,
                                          _config.OutputFileNameWithoutPrefix + ".pot");

            var encoding = string.IsNullOrEmpty(_config.Encoding) ? "" : "--from-code=" + _config.Encoding;

            // http://www.gnu.org/s/hello/manual/gettext/xgettext-Invocation.html
            var args = string.Format("-L{2} -k{3} --omit-header {4} -o\"{0}\" -f\"{1}\"", 
                outputFile, manifest, _config.ProgramLanguage, functions, encoding);

            RunWithOutput(_config.GetTextExecutable, args); // Mark H bodge
        }

        private void RunWithOutput(string filename, string args)
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
            if (!string.IsNullOrEmpty(_config.DryRun)) return;

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

        private static string[] GetFiles(string path, string pattern)
        {
            var files = new List<string>();
            var directories = Directory.GetDirectories(path);

            foreach (var dir in directories.Where(x => !x.EndsWith("\\obj")))
            {
                files.AddRange(Directory.GetFiles(dir, pattern, SearchOption.AllDirectories));
            }

            return files.ToArray();
        }

        private string BuildProjectFileManifest()
        {
            var files = (from path in _config.InputPaths
                         from extension in _config.FileExtensions
                         select GetFiles(path, extension)).SelectMany(s => s);

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