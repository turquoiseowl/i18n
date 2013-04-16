using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using i18n.Domain.Entities;

namespace i18n.Domain.Abstract
{
	/// <summary>
	/// For managing a translation repository for reading, writing and searching. As long as you implement this you can store your translation wherever you want.
	/// </summary>
	public interface ITranslationRepository
	{
		Translation GetLanguage(string tag);
		IEnumerable<Language> GetAvailableLanguages();
		bool TranslationExists(string tag);
		void SaveTranslation(Translation translation);
		void SaveTemplate(IDictionary<string, TemplateItem> items);
	}
}
