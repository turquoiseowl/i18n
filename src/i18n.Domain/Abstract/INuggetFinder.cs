using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using i18n.Domain.Entities;

namespace i18n.Domain.Abstract
{
	/// <summary>
	/// For finding nuggets that needs translating. Likely implementations is FileFinder and DatabaseFinder but anything is possible.
	/// The interface does not help you supplying whitelist for what to check (for instance which database tables/columns) so the existance of this interface is more for testing/mocking.
	/// </summary>
	public interface INuggetFinder
	{
		IDictionary<string, TemplateItem> ParseAll();
	}
}
