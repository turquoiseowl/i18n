using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using i18n.Domain.Abstract;

namespace i18n.Domain.Tests
{
    public class SettingService_Mock : AbstractSettingService
    {
        private readonly IDictionary<string, string> _settings = new Dictionary<string, string>();

        public SettingService_Mock() : base(String.Empty)
        {
            
        }

        public override string GetConfigFileLocation()
        {
            //TODO get exec location
            return Path.Combine(Directory.GetCurrentDirectory(), "SettingService_Mock.cs");
        }

        public override string GetSetting(string key)
        {
            if (_settings.ContainsKey(key))
                return _settings[key];
            return null;
        }

        public override void SetSetting(string key, string value)
        {
            if (!_settings.ContainsKey(key))
                _settings.Add(key, value);
            else
                _settings[key] = value;
        }

        public override void RemoveSetting(string key)
        {
            if (_settings.ContainsKey(key))
                _settings.Remove(key);
        }
    }
}
