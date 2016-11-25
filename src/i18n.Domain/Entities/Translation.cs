using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
    /// <summary>
    /// Holds a complete translation in any one language. The template (TemplateItems) will have told the language which id's/strings that needs translation.
    /// </summary>
    public class Translation
    {
        public Language LanguageInformation { get; set; }
        public virtual ConcurrentDictionary<string, TranslationItem> Items  { get; set; }
    }
}
