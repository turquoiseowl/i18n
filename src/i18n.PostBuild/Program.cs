using System;
using System.Collections;
using System.Collections.Generic;
using i18n.Domain.Concrete;
using i18n.Domain.Entities;

namespace i18n.PostBuild
{
    class Program
    {
        static void Main(string[] args)
        {
			POTranslationRepository rep = new POTranslationRepository(new i18nSettings(new ConfigFileSettingService()));

			NuggetFileParser nugget = new NuggetFileParser(new i18nSettings(new ConfigFileSettingService()));
	        var items = nugget.ParseAll();
	        rep.SaveTemplate(items);

			Translation translation = rep.GetLanguage("fr");
			TranslationSynchronization ts = new TranslationSynchronization(rep);
	        ts.SynchronizeTranslation(items, translation);

            Console.WriteLine("i18n.PostBuild completed successfully.");
        }
    }
}
