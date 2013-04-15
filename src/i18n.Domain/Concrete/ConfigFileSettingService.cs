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
	//todo: We handle both absolute and relative paths so many developers can handle the same project. But right now since config file resides in bin dir that is usually excluded from git/svn each dev still has to config everything. Ideally we would change to allow config file in other location


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

			if (config.AppSettings.Settings[key] == null)
			{
				//create setting
				config.AppSettings.Settings.Add(key, value);
			}
			else
			{
				//make changes
				config.AppSettings.Settings[key].Value = value;	
			}

			
			

			//save to apply changes
			config.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection("appSettings");
		}

	}

}
