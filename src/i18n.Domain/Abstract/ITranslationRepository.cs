using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using i18n.Domain.Entities;

namespace i18n.Domain.Abstract
{
	/// <summary>
	/// For managing a translation repository for reading, writing and searching. As long as you implement this you can store your translation wherever you want.
	/// </summary>
	public interface ITranslationRepository
	{
		Translation GetTranslation(string tag);
		IEnumerable<Language> GetAvailableLanguages();
		bool TranslationExists(string tag);
		void SaveTranslation(Translation translation);
		void SaveTemplate(IDictionary<string, TemplateItem> items);

		//this is for one language so in the case of PO files one messages.po
		CacheDependency GetCacheDependencyLanguage(string tag);  

		//this is for all languages, so if there is a new language. In the case of PO files it's the locale directory
		CacheDependency GetCacheDependencyAllLanguages();
	}
}
