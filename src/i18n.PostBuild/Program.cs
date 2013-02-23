using System;
using System.Configuration;
using System.Reflection;

namespace i18n.PostBuild
{
    class Program
    {
        private const char SplitCharacter = ',';

        static void Main(string[] args)
        {
            var config = ConfigurationFromAppConfig();

            if(args.Length > 0)
            {
                config.OutputPath = args[0];
            }

            if(string.IsNullOrEmpty(config.OutputPath))
            {
                Console.WriteLine("This post build task requires passing in the $(ProjectDirectory) path");
                return;
            }

            for (var i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("gettext:", StringComparison.InvariantCultureIgnoreCase))
                    config.GetTextExecutable = args[i].Substring(8);

                if (args[i].StartsWith("msgmerge:", StringComparison.InvariantCultureIgnoreCase))
                    config.MsgMergeExecutable = args[i].Substring(9);

                if (args[i].StartsWith("inputpaths:", StringComparison.InvariantCultureIgnoreCase))
                    config.InputPaths = args[i].Substring(11).Split(SplitCharacter);
            }

            // Fix output and input paths
            config.OutputPath = config.OutputPath.Trim(new[] { '\"' });

            if (string.IsNullOrEmpty(config.InputPaths[0]))
            {
                config.InputPaths = config.OutputPath.Split(SplitCharacter);
            }

            // Javascript and Coffeescript works with the Python gettext parser.
            if (config.ProgramLanguage.ToLowerInvariant() == "javascript" || config.ProgramLanguage.ToLowerInvariant() == "coffeescript")
            {
                config.ProgramLanguage = "Python";
            }

            new PostBuildTask(config).Execute();
        }

        static PostBuildTaskConfiguration ConfigurationFromAppConfig()
        {
            var outputType = typeof (PostBuildTaskConfiguration);
            var output = new PostBuildTaskConfiguration();

            foreach (var property in outputType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = ConfigurationManager.AppSettings[property.Name];
                if (value == null)
                {
                    Console.WriteLine(string.Format(@"Key ""{0}"" is missing from appSettings in {1}.config.", property.Name, AppDomain.CurrentDomain.FriendlyName));
                    Environment.Exit(1);
                }

                property.SetValue(output, property.PropertyType == typeof (string[]) ? (object) value.Split(SplitCharacter) : value, null);
            }

            return output;
        }
    }
}
