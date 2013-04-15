using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
	/// <summary>
	/// Keeps all information about a language that can be handy when listing languages.
	/// </summary>
	public class Language
	{
		public string LanguageShortTag { get; set; }
		public string LanguageInEnglish { get; set; }
		public string LanguageInLocal { get; set; }
	}
}
