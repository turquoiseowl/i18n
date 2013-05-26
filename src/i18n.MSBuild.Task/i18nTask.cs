using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace i18n.MSBuild.Task
{
    /// <summary>
    /// A task for building a localization message database using GNU xgettext.
    /// <example>
    /// <!-- Add target to your main .csproj file. -->
    /// <Target Name="AfterBuild">
    ///     <ItemGroup>
    ///       <iProjectDirectories Include="$(ProjectDir)" />
    ///     </ItemGroup>
    ///     <i18nTask 
    ///         ProjectDirectories="@(iProjectDirectories)"
    ///         MsgMerge="http://www.gnu.org/s/hello/manual/gettext/msgmerge-Invocation.html" 
    ///         GetText="http://www.gnu.org/s/hello/manual/gettext/xgettext-Invocation.html" />
    /// </Target>
    /// </example>
    /// </summary>
    public class i18nTask : Microsoft.Build.Utilities.Task
    {

        /// <summary>
        /// Path to gettext library.
        /// </summary>
        private static readonly string GetTextPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gettext"));

        /// <summary>
        /// Project directories that will be scanned by gettext.
        /// </summary>
        [Required]
        public ITaskItem[] ProjectDirectories { get; set; }

        /// <summary>
        /// Translation output directory.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Additional msgmerge parameters.
        /// </summary>
        public string MsgMerge { get; set; }

        /// <summary>
        /// Additional gettext parameters.
        /// </summary>
        public string GetText { get; set; }

        /// <summary>
        /// Staging area for extracting gettext translations. 
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {

            // Debug this task.
            // Debugger.Launch();

            var projectDirectories = new string[ProjectDirectories.Length];
            for (int i = 0; i < projectDirectories.Length; i++)
            {
                projectDirectories[i] = ToAbsolutePath(ProjectDirectories[i].ItemSpec);
                if (!Directory.Exists(projectDirectories[i]))
                {
                    throw new DirectoryNotFoundException("Invalid project directory {0}".FormatWith(projectDirectories[i]));
                }
                Log.LogMessage(MessageImportance.Normal, "Added project directory {0}", projectDirectories[i]);
            }

            if (string.IsNullOrEmpty(OutputPath))
            {
                OutputPath = Path.Combine(BuildEngine.ProjectFileOfTaskNode, "locale");
                Log.LogWarning("Warning output directory not specified. Defaulting to {0}", OutputPath);
            }

            OutputPath = ToAbsolutePath(OutputPath);

            try
            {
                ExtractTranslations(OutputPath, GetText, MsgMerge, projectDirectories);
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }

            Log.LogMessage(MessageImportance.Normal, "Succesfully extracted i18n " +
                                                     "translations from {0} projects sources.", projectDirectories.Length);

            return true;

        }

        /// <summary>
        /// Extract translations from specified project directories.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="gettext"></param>
        /// <param name="msgmerge"></param>
        /// <param name="inputPaths"></param>
        private void ExtractTranslations(string outputPath, string gettext = null,
            string msgmerge = null, string[] inputPaths = null)
        {

            string localePath = Path.Combine(outputPath, "locale");

            // create locale directory?
            if (!Directory.Exists(localePath))
            {
                Log.LogWarning("Locale directory {0} does not exist, will attempt to create.", outputPath);
                try
                {
                    Directory.CreateDirectory(localePath);
                    Log.LogMessage(MessageImportance.Normal, "Created locale directory {0}", localePath);
                }
                catch (Exception ex)
                {
                    Log.LogWarning("Error occured while attempting to create " +
                                   "locale directory {0}. Reason: {1}", ex.Message);
                    return;
                }
            }

            // list of files to extract gettext strings from
            var manifest = BuildProjectFileManifest(inputPaths);

            // extract te
            CreateMessageTemplate(outputPath, manifest, gettext);

            // merge template with existing locales
            MergeTemplateWithExistingLocales(outputPath, localePath, msgmerge);

            // delete temporary manifest file
            File.Delete(manifest);
        }

        /// <summary>
        /// Merge two .po files together.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="localePath"> </param>
        /// <param name="options"></param>
        private void MergeTemplateWithExistingLocales(string outputPath, string localePath, string options)
        {
            // todo: if pot does not exist in locale directory, should we throw or return?
            var messagePot = Path.Combine(outputPath, @"locale\messages.pot");

            // all language directories
            IEnumerable<string> locales = Directory.GetDirectories(localePath);            
            
            // merge pot with each localized po
            foreach (var messagePo in locales.Select(locale => Path.Combine(locale, "messages.po")))
            {
                if (File.Exists(messagePo))
                {
                    // http://www.gnu.org/s/hello/manual/gettext/msgmerge-Invocation.html
                    var args = string.Format("{2} -U \"{0}\" \"{1}\"", messagePo, messagePot, options);
                    RunWithOutput(Path.Combine(GetTextPath, "msgmerge.exe"), args);
                    Log.LogMessage("Merging messages.pot with {1}", messagePot, messagePo);
                }
                else
                {
                    File.Copy(messagePot, messagePo);
                }
            }

        }

        /// <summary>
        /// Create messages.pot for all extracted gettext strings.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="manifest"></param>
        /// <param name="options"></param>
        private void CreateMessageTemplate(string outputPath, string manifest, string options)
        {
            // http://www.gnu.org/s/hello/manual/gettext/xgettext-Invocation.html
            outputPath = Path.Combine(outputPath, @"locale\messages.pot");
            var args = string.Format(
                "{2} -LC# -k_ --omit-header --from-code=UTF-8 -o\"{0}\" -f\"{1}\"",
                outputPath, manifest, options);
            RunWithOutput(Path.Combine(GetTextPath, "xgettext.exe"), args); // Mark H bodge
        }

        /// <summary>
        /// Execute native gettext executable.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="args"></param>
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

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename);
            }

            var process = Process.Start(info);
            var stderr = new StringBuilder();
            var stdout = new StringBuilder();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.OutputDataReceived += (sender, eventArgs) => 
                stdout.Append(eventArgs.Data);
            process.ErrorDataReceived += (sender, eventArgs) => 
                stderr.Append(eventArgs.Data);

            process.WaitForExit();

            if (Debugger.IsAttached)
            {
                Log.LogMessage(MessageImportance.Normal,
                    string.Format("[stdout ({0})]: {1}", Path.GetFileName(filename), stdout));
            }

            // posix return codes
            // http://www.gnu.org/software/libc/manual/html_node/Exit-Status.html
            if (process.ExitCode != 0)
            {
                throw new GetTextProcessException(filename, process.ExitCode, stderr);
            }

        }

        /// <summary>
        /// Find all files in paths that matches a specific pattern.
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private IEnumerable<string[]> GetFiles(IEnumerable<string> paths, string pattern)
        {
            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    Log.LogWarning("Unable to search for files in directory {0} because it does not exist.", path);
                    continue;
                }
                Log.LogMessage(MessageImportance.High, "Scanning directory {0} for gettext strings.", path);
                yield return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            }
        }

        /// <summary>
        /// Build a manifest that contains all files gettext should
        /// try extracting strings from.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private string BuildProjectFileManifest(string[] paths)
        {
            var cs = GetFiles(paths, "*.cs");
            var razor = GetFiles(paths, "*.cshtml");
            var files = cs.Union(razor).SelectMany(f => f).ToList();

            var temp = Path.GetTempFileName();
            using (var sw = File.CreateText(temp))
            {
                foreach (var file in files)
                {
                    sw.WriteLine(file);
                }
            }

            return ToAbsolutePath(temp);
        }

        /// <summary>
        /// Convert a windows path to absolute path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string ToAbsolutePath(string path)
        {
            return Path.GetFullPath(path);
        }

    }
}
