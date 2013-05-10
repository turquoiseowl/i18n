using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using i18n.Domain.Abstract;
using System.Web.Configuration;
using System.Configuration;

namespace i18n.Domain.Concrete
{
	//todo: We handle both absolute and relative paths so many developers can handle the same project. But right now since config file resides in bin dir that is usually excluded from git/svn each dev still has to config everything. Ideally we would change to allow config file in other location


	public class WebConfigSettingService : AbstractSettingService
	{
		private Configuration _configuration;
		private AppSettingsSection _settings;

		public WebConfigSettingService(string configLocation = null) : base(configLocation)
		{
			//http://stackoverflow.com/questions/4738/using-configurationmanager-to-load-config-from-an-arbitrary-location/4746#4746
			try
			{


				if (configLocation != null)
				{
					//ConfigurationFileMap fileMap = new ConfigurationFileMap(configLocation); //Path to your config file
					//_configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
					ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
					fileMap.ExeConfigFilename = configLocation;
					_configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
					//_settings = (AppSettingsSection)_configuration.GetSection("AppSettings");
					_settings = _configuration.AppSettings;
				}
				else
				{
					//No config file was sent in so we use default one
					_configuration = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current != null ? HttpContext.Current.Request.ApplicationPath : null);
					_settings = _configuration.AppSettings;
				}
			}
			catch (ConfigurationErrorsException e)
			{
				var eNew = new ConfigurationErrorsException("Could not load configuration. Either incorrect path was sent in or if no path was sent in web.config could not be found where expected",e);
				throw eNew;
			}
		}

		public override string GetConfigFileLocation()
		{
			return _configuration.FilePath;
		}

		public override string GetSetting(string key)
		{
			string setting;
			if (_settings.Settings[key] != null)
			{
				setting = _settings.Settings[key].Value;

				if (!string.IsNullOrEmpty(setting))
				{
					return setting;
				}	
			}
			

			return null;
		}

		public override void SetSetting(string key, string value)
		{

			if (_settings.Settings[key] == null)
			{
				//create setting
				_settings.Settings.Add(key, value);
			}
			else
			{
				//make changes
				_settings.Settings[key].Value = value;
			}

			//save to apply changes
			_configuration.Save(ConfigurationSaveMode.Modified);
		}

		public override void RemoveSetting(string key)
		{
			_settings.Settings.Remove(key);
		}
	}

}
