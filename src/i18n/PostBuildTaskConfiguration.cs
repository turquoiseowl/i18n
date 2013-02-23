namespace i18n
{
    /// <summary>
    /// Configuration for the PostBuildTask class.
    /// </summary>
    public class PostBuildTaskConfiguration
    {
        public string OutputPath { get; set; }
        public string[] InputPaths { get; set; }

        public string GetTextExecutable { get; set; }
        public string MsgMergeExecutable { get; set; }
        public string LocaleDirectoryName { get; set; }
        public string OutputFileNameWithoutPrefix { get; set; }
        public string[] FileExtensions { get; set; }

        public string ProgramLanguage { get; set; }
        public string[] TranslationFunctions { get; set; }
        public string Encoding { get; set; }

        /// <summary>
        /// Set to any value but blank if no execution should be made.
        /// </summary>
        public string DryRun { get; set; }
    }
}