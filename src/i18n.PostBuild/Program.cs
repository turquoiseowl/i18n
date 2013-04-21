using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using i18n.Domain.Concrete;
using i18n.Domain.Entities;

namespace i18n.PostBuild
{
    class Program
    {
        static void Main(string[] args)
        {
			//TestCode
	        //args = new string[] {@"C:\viducate2\Viducate\Viducate.WebUI\Web.config"};

			string configPath;
			if (args.Length == 0)
			{
				System.Console.WriteLine("You have to specify path to web.config.");
				return;
			}

	        try
	        {
		        configPath = args[0];
				using (FileStream fs = File.Open(configPath, FileMode.Open))
		        {
			        
		        }
	        }
	        catch (Exception)
	        {
				System.Console.WriteLine("Failed to open config file at path.");
				return;
	        }

			//todo: this assumes PO files, if not using po files then other solution needed.
			i18nSettings settings = new i18nSettings(new WebConfigSettingService(configPath));
			POTranslationRepository rep = new POTranslationRepository(settings);

			FileNuggetFinder nugget = new FileNuggetFinder(settings);
	        var items = nugget.ParseAll();
	        rep.SaveTemplate(items);

			TranslationMerger ts = new TranslationMerger(rep);
			ts.MergeAllTranslation(items);
			

            Console.WriteLine("i18n.PostBuild completed successfully.");
        }
    }
}
