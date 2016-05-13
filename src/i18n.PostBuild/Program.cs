using System;
using System.IO;
using i18n.Domain.Concrete;
using i18n.Domain.Entities;
using i18n.Domain.Helpers;

namespace i18n.PostBuild
{
    public class Program
    {
        public bool ShowHelp { get; set; }
        public bool ShowSourceContext { get; set; }
        public string ConfigPath { get; set; }

        public static void Main(string[] args)
        {
            Environment.ExitCode = 1;

            try
            {
                var program = new Program();
                program.ParseArguments(args);

                if (program.ShowHelp)
                {
                    ShowUsage();
                }
                else
                {
                    program.Run();
                }

                Environment.ExitCode = 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR: {0}", exception.Message);
            }
        }

        private void ParseArguments(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    var option = arg.Substring(1);

                    switch (option.ToLowerInvariant())
                    {
                        case "help":
                            ShowHelp = true;
                            break;

                        case "source":
                            ShowSourceContext = true;
                            break;

                        default:
                            throw new Exception("Unknown option: " + arg);
                    }

                }
                else if (string.IsNullOrWhiteSpace(ConfigPath))
                {
                    ConfigPath = arg;
                }
                else
                {
                    throw new Exception("Can only specify one configPath");
                }
            }

            ShowHelp = ShowHelp || string.IsNullOrWhiteSpace(ConfigPath);
        }

        private static void ShowUsage()
        {
            Console.WriteLine("usage: i18n.PostBuild [options] configPath");
            Console.WriteLine();
            Console.WriteLine("where: configPath - path to web.config file");
            Console.WriteLine("       /help      - show this message");
            Console.WriteLine("       /source    - append source context to references");
            Console.WriteLine();
        }

        private void Run()
        {
            ThrowIfConfigFileNotFound();

            ReferenceContext.ShowSourceContext = ShowSourceContext;

			//todo: this assumes PO files, if not using po files then other solution needed.
			var settings = new i18nSettings(new WebConfigSettingService(ConfigPath));
			var repository = new POTranslationRepository(settings);

			var nuggetFinder = new FileNuggetFinder(settings);
	        var items = nuggetFinder.ParseAll();
	        repository.SaveTemplate(items);

			var merger = new TranslationMerger(repository);
			merger.MergeAllTranslation(items);

            Console.WriteLine("i18n.PostBuild completed successfully.");
        }

        private void ThrowIfConfigFileNotFound()
        {
            try
            {
                using (File.Open(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to open config file: {0}", ConfigPath);
                throw;
            }
        }
    }
}
