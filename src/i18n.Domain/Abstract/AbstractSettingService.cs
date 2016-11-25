using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Abstract
{
    /// <summary>
    /// Abstract class for settings, we require being able to fetch all settings, finding one and also saving.
    /// </summary>
    abstract public class AbstractSettingService
    {
        private string _configFileLocation;

        /// <summary>
        /// ctor allowing custom config file
        /// </summary>
        /// <param name="configFileLocation">Link to the config files location. If set to null, settings will try to work out path on it's own</param>
        public AbstractSettingService(string configFileLocation)
        {
            _configFileLocation = configFileLocation;
        }

        public abstract string GetConfigFileLocation();
        abstract public string GetSetting(string key);
        abstract public void SetSetting(string key, string value);
        abstract public void RemoveSetting(string key);
    }

}
