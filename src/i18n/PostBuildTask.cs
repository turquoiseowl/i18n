using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace i18n
{
    /// <summary>
    /// A post-build task for building a localization message database using GNU xgettext
    /// <see href="http://gnuwin32.sourceforge.net/packages/gettext.htm" />
    /// </summary>
    public class PostBuildTask
    {
        private const string cTransformedFileExtension = "PostBuildBak";
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

            string manifest = null;
            try
            {
                manifest = BuildProjectFileManifest(inputPaths);

                CreateMessageTemplate(outputPath, manifest, gettext);

                MergeTemplateWithExistingLocales(outputPath, msgmerge);
            }
            finally
            {
                if (manifest != null) File.Delete(manifest);
                RestoreTransformBackups(inputPaths);
            }
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
            var args = string.Format("{2} -LC# -k_ -k__ --omit-header --from-code=UTF-8 -o\"{0}\\locale\\messages.pot\" -f\"{1}\"", outputPath, manifest, options);
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
            TransformAnnotations(cs);
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

        private void RestoreTransformBackups(string[] paths)
        {
            var files = GetFiles(paths, "*.cs" + cTransformedFileExtension);
            foreach (var file in files)
            {
                var originalFileName = file.Remove(file.Length - cTransformedFileExtension.Length);
                File.Delete(originalFileName);
                File.Move(file, originalFileName);
            }
        }

        private static void TransformAnnotations(string[] paths)
        {
            foreach (var path in paths)
            {
                string fileText = File.ReadAllText(path, encoding: System.Text.Encoding.UTF8);
                string transformed = null;
                if (TransformAnnotations(fileText, out transformed))
                {
                    if (!File.Exists(path + cTransformedFileExtension))
                    {
                        File.WriteAllText(path: path + cTransformedFileExtension, contents: fileText, encoding: System.Text.Encoding.UTF8);
                        //backup last modified date, so that visual studio doesnt prompt to reload the view model after build
                        File.SetLastWriteTime(path: path + cTransformedFileExtension,
                            lastWriteTime: File.GetLastWriteTime(path));
                    }
                    File.WriteAllText(path: path, contents: transformed, encoding: System.Text.Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// Transform annotations in code file to format than can be processed by xgettext
        /// </summary>
        /// <param name="fileText">original code file</param>
        /// <param name="transformed">transformed code file</param>
        /// <returns>true if input file contained any annotations that were transformed</returns>
        private static bool TransformAnnotations(string fileText, out string transformed)
        {
            Regex displayName = new Regex(@"\[(i18n.DataAnnotations.)?Display\((?<Property>.*)Name\s?=\s?(?<String>\""[^\""]*\"")(?<Others>.*)");
            Regex displayPrompt = new Regex(@"\[(i18n.DataAnnotations.)?Display\((?<Property>.*)Prompt\s?=\s?(?<String>\""[^\""]*\"")(?<Others>.*)");
            Regex displayDescription = new Regex(@"\[(i18n.DataAnnotations.)?Display\((?<Property>.*)Description\s?=\s?(?<String>\""[^\""]*\"")(?<Others>.*)");
            Regex dataType = new Regex(@"\[(i18n.DataAnnotations.)?DataType\((?<Property>.*)ErrorMessage\s?=\s?(?<String>\""[^\""]*\"")(?<Others>.*)\)");
            Regex required = new Regex(@"\[(i18n.DataAnnotations.)?Required\((?<Property>.*)ErrorMessage\s?=\s?(?<String>\""[^\""]*\"")(?<Others>.*)\)");
            Regex stringLength = new Regex(@"\[(i18n.DataAnnotations.)?StringLength\((?<Property>.*)ErrorMessage\s?=\s?(?<String>\""[^\""]*\"")(?<Others>.*)\)");
            transformed = fileText;
            bool localizeAnnotations = false;

            if (displayName.IsMatch(transformed))
            {
                transformed = displayName.Replace(input: transformed, replacement: @"[Display(${Property}_(${String})${Others}");
                localizeAnnotations = true;
            }
            if (displayPrompt.IsMatch(transformed))
            {
                transformed = displayPrompt.Replace(input: transformed, replacement: @"[Display(${Property}_(${String})${Others}");
                localizeAnnotations = true;
            }
            if (displayDescription.IsMatch(transformed))
            {
                transformed = displayDescription.Replace(input: transformed, replacement: @"[Display(${Property}_(${String})${Others}");
                localizeAnnotations = true;
            }
            if (dataType.IsMatch(transformed))
            {
                transformed = dataType.Replace(input: transformed, replacement: @"[DataType(${Property}_(${String})${Others})");
                localizeAnnotations = true;
            }
            if (required.IsMatch(transformed))
            {
                transformed = required.Replace(input: transformed, replacement: @"[Required(${Property}_(${String})${Others})");
                localizeAnnotations = true;
            }
            if (stringLength.IsMatch(transformed))
            {
                transformed = stringLength.Replace(input: transformed, replacement: @"[StringLength(${Property}_(${String})${Others})");
                localizeAnnotations = true;
            }

            return localizeAnnotations;
        }
    }
}
