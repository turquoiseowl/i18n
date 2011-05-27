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
            var text = File.ReadAllText(manifest);
            Trace.WriteLine(text);

            // http://www.gnu.org/s/hello/manual/gettext/xgettext-Invocation.html
            var args = string.Format("-LC# -k_ --omit-header --from-code=UTF-8 -o\"{0}\\locale\\messages.pot\" -f\"{1}\"", path, manifest);
            var info = new ProcessStartInfo("gettext\\xgettext.exe", args)
                            {
                                UseShellExecute = false,
                                ErrorDialog = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };

            RunGetText(args, info);

            File.Delete(manifest);
        }

        private static void RunGetText(string args, ProcessStartInfo info)
        {
            Trace.WriteLine(string.Format("gettext\\xgettext.exe {0}", args));
            var process = Process.Start(info);
            while (!process.StandardError.EndOfStream)
            {
                var line = process.StandardError.ReadLine();
                if (line == null) continue;
                Trace.WriteLine(line);
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