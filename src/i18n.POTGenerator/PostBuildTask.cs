using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VSPackage.i18n_POTGenerator
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
        public void Execute(string path, string gettext = null, string msgmerge = null)
        {
            var manifest = BuildProjectFileManifest(path);

            CreateMessageTemplate(path, manifest, gettext);

            MergeTemplateWithExistingLocales(path, msgmerge);

            File.Delete(manifest);
        }

        private static void MergeTemplateWithExistingLocales(string path, string options)
        {
            var locales = Directory.GetDirectories(string.Format("{0}\\locale\\", path));
            var template = string.Format("{0}\\locale\\messages.pot", path);

            foreach (var messages in locales.Select(locale => string.Format("{0}\\messages.po", locale)))
            {
                if(File.Exists(messages))
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

        private static void CreateMessageTemplate(string path, string manifest, string options)
        {
            // http://www.gnu.org/s/hello/manual/gettext/xgettext-Invocation.html
            var args = string.Format("{2} -LC# -k_ -k__ --omit-header --from-code=UTF-8 -o\"{0}\\locale\\messages.pot\" -f\"{1}\"", path, manifest, options);
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

		private static string[] GetFiles(string path, string pattern) {
			var files = new List<string>();

			var directories = Directory.GetDirectories( path );

			foreach ( var dir in directories.Where( x => !x.EndsWith( "\\obj" ) ) ) {
				files.AddRange( Directory.GetFiles( dir, pattern, SearchOption.AllDirectories ) );
			}


			return files.ToArray();
		}

        private static string BuildProjectFileManifest(string path)
        {
			var cs = GetFiles( path, "*.cs" );
            var razor = GetFiles(path, "*.cshtml");
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
