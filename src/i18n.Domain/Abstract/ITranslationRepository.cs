using System.Collections.Generic;
using System.Web.Caching;
    // TODO: this above dependency is unfortunate and should be removed.
    // That would involve a reworking of the design for notifications
    // of languages being modified.
    // GetCacheDependencyForSingleLanguage could be replaced with an event in the Translation
    // object which is signalled when that particular translation becomes dirty.
    // Like wise GetCacheDependencyForAllLanguages could return just an event.
    // It is then down to the client to wrap these events in a custom CacheDependency
    // that monitors the event.
using i18n.Domain.Entities;

namespace i18n.Domain.Abstract
{
	/// <summary>
	/// For managing a translation repository for reading, writing and searching. As long as you implement this you can store your translation wherever you want.
	/// </summary>
	public interface ITranslationRepository
	{
		Translation GetTranslation(string langtag);
		IEnumerable<Language> GetAvailableLanguages();
		bool TranslationExists(string langtag);
		void SaveTranslation(Translation translation);
		void SaveTemplate(IDictionary<string, TemplateItem> items);

		//this is for one language so in the case of PO files one messages.po
		CacheDependency GetCacheDependencyForSingleLanguage(string langtag);  

		//this is for all languages, so if there is a new language. In the case of PO files it's the locale directory
		CacheDependency GetCacheDependencyForAllLanguages();
	}
}
