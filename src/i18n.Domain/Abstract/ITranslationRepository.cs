using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using i18n.Domain.Entities;

namespace i18n.Domain.Abstract
{
	interface ITranslationRepository
	{
		IQueryable<TranslateItem> GetLanguage(LanguageTag tag);
		ConcurrentDictionary<string, TranslateItem> GetLanguageDictionary(LanguageTag tag);
		IEnumerable<Language> GetAvailableLanguages();
		bool TranslationExists(LanguageTag tag);
	}
}
