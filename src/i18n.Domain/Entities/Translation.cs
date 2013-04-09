using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
	public class Translation
	{
		public Language LanguageInformation { get; set; }
		public virtual IEnumerable<TranslateItem> Items  { get; set; }
	}
}
