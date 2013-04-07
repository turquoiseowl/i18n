using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
	public class TemplateItem
	{
		public string Id;
		public IEnumerable<string> References { get; set; }
		public IEnumerable<string> Comments { get; set; }
	}
}
