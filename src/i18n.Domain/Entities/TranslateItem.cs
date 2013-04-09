using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
	public class TranslateItem
	{
		//todo: The po specification actually says that if you want to multiline the message (and possibly the id) it should looks like this
		//"this is line 1 of comment \n""
		//"this is line two"
		//this of course means that messages needs to be a collection and the POTranslationRepository's parse and save functions needs to be updated.
		//this should preferably be decided upon and fixed before anyone writes a database repository
	
		public string Id { get; set; }
		public string Message { get; set; }
		public IEnumerable<string> References { get; set; }
		public IEnumerable<string> ExtractedComments { get; set; }
		public IEnumerable<string> TranslatorComments { get; set; }
		public IEnumerable<string> Flags { get; set; }
	}
}
