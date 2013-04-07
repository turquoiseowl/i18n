using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using i18n.Domain.Abstract;

namespace i18n.Domain.Concrete
{
	class i18nSettings
	{
		private ISettingService _settingService;
		private const string _prefix = "i18n.";

		public i18nSettings(ISettingService settings)
		{
			_settingService = settings;
		}

		private string GetPrefixedString(string key)
		{
			return _prefix + key;
		}

		#region Locale directory

		private const string _localeDirectoryRelativePathDefault = "locale";
		public virtual string LocaleDirectoryRelativePath
		{
			get
			{
				string prefixedString = GetPrefixedString("LocaleDirectoryRelativePath");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting;	
				}
				else
				{
					_settingService.SetSetting(prefixedString, _localeDirectoryRelativePathDefault);
					return _localeDirectoryRelativePathDefault;
				}
				
			}
			set
			{
				string prefixedString = GetPrefixedString("LocaleDirectoryRelativePath");
				_settingService.SetSetting(prefixedString, value);
			}
		}

		#endregion


		#region White list

		private const string _whiteListDefault = "*.cs;*.js";
		public virtual IEnumerable<string> WhiteList
		{
			get
			{
				string prefixedString = GetPrefixedString("WhiteList");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting.Split(';').ToList();
				}
				else
				{
					_settingService.SetSetting(prefixedString, _whiteListDefault);
					return _whiteListDefault.Split(';').ToList();
				}
			}
			set
			{
				string prefixedString = GetPrefixedString("WhiteList");
				_settingService.SetSetting(prefixedString, string.Join(";", value));
			}
		}

		#endregion


		#region Nugget tokens

		private const string _nuggetBeginTokenDefault = "[[[";
		public virtual string NuggetBeginToken
		{
			get
			{
				string prefixedString = GetPrefixedString("NuggetBeginToken");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting;
				}
				else
				{
					_settingService.SetSetting(prefixedString, _nuggetBeginTokenDefault);
					return _nuggetBeginTokenDefault;
				}

			}
			set
			{
				string prefixedString = GetPrefixedString("NuggetBeginToken");
				_settingService.SetSetting(prefixedString, value);
			}
		}

		private const string _nuggetEndTokenDefault = "]]]";
		public virtual string NuggetEndToken
		{
			get
			{
				string prefixedString = GetPrefixedString("NuggetEndToken");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting;
				}
				else
				{
					_settingService.SetSetting(prefixedString, _nuggetEndTokenDefault);
					return _nuggetEndTokenDefault;
				}

			}
			set
			{
				string prefixedString = GetPrefixedString("NuggetEndToken");
				_settingService.SetSetting(prefixedString, value);
			}
		}

		private const string _nuggetDelimiterTokenDefault = "|||";
		public virtual string NuggetDelimiterToken
		{
			get
			{
				string prefixedString = GetPrefixedString("NuggetDelimiterToken");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting;
				}
				else
				{
					_settingService.SetSetting(prefixedString, _nuggetDelimiterTokenDefault);
					return _nuggetDelimiterTokenDefault;
				}

			}
			set
			{
				string prefixedString = GetPrefixedString("NuggetDelimiterToken");
				_settingService.SetSetting(prefixedString, value);
			}
		}

		#endregion


		#region DirectoriesToScan

		private const string _directoriesToScan = "../../";
		public virtual IEnumerable<string> DirectoriesToScan
		{
			get
			{
				string prefixedString = GetPrefixedString("DirectoriesToScan");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting.Split(';').ToList();
				}
				else
				{
					_settingService.SetSetting(prefixedString, _directoriesToScan);
					return _directoriesToScan.Split(';').ToList();
				}
			}
			set
			{
				string prefixedString = GetPrefixedString("DirectoriesToScan");
				_settingService.SetSetting(prefixedString, string.Join(";", value));
			}
		}

		#endregion
	}
}
