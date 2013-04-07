using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
	public class TranslateItem
	{
		public string Id { get; set; }
		public string Message { get; set; }
		public IEnumerable<string> References { get; set; }
		public IEnumerable<string> ExtractedComments { get; set; }
		public IEnumerable<string> TranslatorComments { get; set; }
		public IEnumerable<string> Flags { get; set; }
	}
}
