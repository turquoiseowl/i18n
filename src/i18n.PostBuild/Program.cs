using System;
using i18n.Domain.Concrete;
using i18n.Domain.Entities;

namespace i18n.PostBuild
{
    class Program
    {
        static void Main(string[] args)
        {
			POTranslationRepository rep = new POTranslationRepository(new i18nSettings(new ConfigFileSettingService()));
	      //  Translation translation = rep.GetLanguage("sv");
			//rep.SaveTranslation(translation);

			NuggetFileParser nugget = new NuggetFileParser(new i18nSettings(new ConfigFileSettingService()));
	        rep.SaveTemplate(nugget.ParseAll());
			
	        

            if(args.Length == 0)
            {
                Console.WriteLine("This post build task requires passing in the $(ProjectDirectory) path");
                return;
            }

            var path = args[0];
            path = path.Trim(new[] {'\"'});

            string gettext = null;
            string msgmerge = null;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("gettext:", StringComparison.InvariantCultureIgnoreCase))
                    gettext = args[i].Substring(8);

                if (args[i].StartsWith("msgmerge:", StringComparison.InvariantCultureIgnoreCase))
                    msgmerge = args[i].Substring(9);
            }

            //new PostBuildTask().Execute(path, gettext, msgmerge);


        }
    }
}
