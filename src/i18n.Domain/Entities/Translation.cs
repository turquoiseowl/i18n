using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
	public class Translation
	{
		public Language LanguageInformation { get; set; }
		public virtual ConcurrentDictionary<string, TranslateItem> Items  { get; set; }
	}
}
