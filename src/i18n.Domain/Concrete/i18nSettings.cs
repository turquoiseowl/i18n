using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using i18n.Domain.Abstract;

namespace i18n.Domain.Concrete
{
	public class i18nSettings
	{
		private AbstractSettingService _settingService;
		private const string _prefix = "i18n.";

		public i18nSettings(AbstractSettingService settings)
		{
			_settingService = settings;
		}

		private string GetPrefixedString(string key)
		{
			return _prefix + key;
		}


		private string MakePathAbsoluteAndFromConfigFile(string path)
		{
			if (Path.IsPathRooted(path))
			{
				return path;
			}
			else
			{
				var startPath = Path.GetDirectoryName(_settingService.GetConfigFileLocation());
				return Path.GetFullPath(Path.Combine(startPath, path));
			}
		}


		#region Locale directory

		private const string _localeDirectoryDefault = "locale";
		public virtual string LocaleDirectory
		{
			get
			{
				string prefixedString = GetPrefixedString("LocaleDirectory");
				string setting = _settingService.GetSetting(prefixedString);
				string path;
				if (setting != null)
				{
					path = setting;	
				}
				else
				{
					path = _localeDirectoryDefault;
				}

				return MakePathAbsoluteAndFromConfigFile(path);
			}
			set
			{
				string prefixedString = GetPrefixedString("LocaleDirectory");
				_settingService.SetSetting(prefixedString, value);
			}
		}

		#endregion


		#region White list

		private const string _whiteListDefault = "*.cs;*.cshtml";
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
					return _nuggetDelimiterTokenDefault;
				}

			}
			set
			{
				string prefixedString = GetPrefixedString("NuggetDelimiterToken");
				_settingService.SetSetting(prefixedString, value);
			}
		}

		private const string _nuggetCommentTokenDefault = "///";
		public virtual string NuggetCommentToken
		{
			get
			{
				string prefixedString = GetPrefixedString("NuggetCommentToken");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting;
				}
				else
				{
					return _nuggetCommentTokenDefault;
				}

			}
			set
			{
				string prefixedString = GetPrefixedString("NuggetCommentToken");
				_settingService.SetSetting(prefixedString, value);
			}
		}

		#endregion


		#region DirectoriesToScan

		private const string _directoriesToScan = ".";
		public virtual IEnumerable<string> DirectoriesToScan
		{
			get
			{
				string prefixedString = GetPrefixedString("DirectoriesToScan");
				string setting = _settingService.GetSetting(prefixedString);
				List<string> list;
				if (setting != null)
				{
					list = setting.Split(';').ToList();
				}
				else
				{
					list = _directoriesToScan.Split(';').ToList();
				}

				List<string> returnList = new List<string>();
				foreach (var path in list)
				{
					returnList.Add(MakePathAbsoluteAndFromConfigFile(path));
				}

				return returnList;
			}
			set
			{
				string prefixedString = GetPrefixedString("DirectoriesToScan");
				_settingService.SetSetting(prefixedString, string.Join(";", value));
			}
		}

		#endregion


		#region Available Languages

		//If empty string is returned the repository can if it choses enumerate languages in a different way (like enumerating directories in the case of PO files)
		//empty string is returned as an IEnumerable with one empty element
		private const string _availableLanguages = "";
		public virtual IEnumerable<string> AvailableLanguages
		{
			get
			{
				string prefixedString = GetPrefixedString("AvailableLanguages");
				string setting = _settingService.GetSetting(prefixedString);
				if (setting != null)
				{
					return setting.Split(';').ToList();
				}
				else
				{
					return _availableLanguages.Split(';').ToList();
				}
			}
			set
			{
				string prefixedString = GetPrefixedString("AvailableLanguages");
				_settingService.SetSetting(prefixedString, string.Join(";", value));
			}
		}

		#endregion

	}
}
