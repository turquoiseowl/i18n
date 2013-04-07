using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using i18n.Domain.Abstract;
using System.Web.Configuration;
using System.Configuration;

namespace i18n.Domain.Concrete
{
	public class ConfigFileSettingService : ISettingService
	{

		public IDictionary<string, string> GetAllSettings()
		{
			var settings = new Dictionary<string, string>();
			var appSettings = ConfigurationManager.AppSettings;

			foreach (var setting in appSettings.AllKeys)
			{
				settings.Add(setting, appSettings[setting]);
			}

			return settings;
		}

		public string GetSetting(string key)
		{
			var setting = ConfigurationManager.AppSettings[key];
			if (!string.IsNullOrEmpty(setting))
			{
				return setting;
			}

			return null;
		}

		public void SetSetting(string key, string value)
		{
			//Create the object
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			//make changes
			config.AppSettings.Settings[key].Value = value;

			//save to apply changes
			config.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection("appSettings");
		}

	}

}
