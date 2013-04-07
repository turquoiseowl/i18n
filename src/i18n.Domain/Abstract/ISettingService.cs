using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Abstract
{
	public interface ISettingService
	{
		IDictionary<string, string> GetAllSettings();
		string GetSetting(string key);
		void SetSetting(string key, string value);
	}

}
