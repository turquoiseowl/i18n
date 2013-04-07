using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
	class Translation
	{
		public string LanguageInEnglish { get; set; }
		public string LanguageInLocal { get; set; }
		public LanguageTag LanguageShortTag { get; set; }
		public virtual IEnumerable<TranslateItem> Items  { get; set; }
	}
}
