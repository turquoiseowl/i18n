using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using i18n.Domain.Entities;
using i18n.Helpers;

namespace i18n.Domain.Concrete
{
	public class NuggetFileParser
	{
		private i18nSettings _settings;

        private NuggetParser _nuggetParser;

		public NuggetFileParser(i18nSettings settings)
		{
			_settings = settings;
            _nuggetParser = new NuggetParser(new NuggetTokens(
			    _settings.NuggetBeginToken,
			    _settings.NuggetEndToken,
			    _settings.NuggetDelimiterToken,
			    _settings.NuggetCommentToken));
		}

		public IDictionary<string, TemplateItem> ParseAll()
		{
			IEnumerable<string> fileWhiteList = _settings.WhiteList;
			IEnumerable<string> directoriesToSearchRecursively = _settings.DirectoriesToScan;

			var templateItems = new ConcurrentDictionary<string, TemplateItem>();
                // Collection of template items keyed by their id.
			string absoluteDirectoryPath;

			foreach (var directoryPath in directoriesToSearchRecursively)
			{
				if (Path.IsPathRooted(directoryPath))
				{
					absoluteDirectoryPath = directoryPath;
				}
				else
				{
					absoluteDirectoryPath = Path.GetFullPath(directoryPath);
				}


				foreach (string filePath in Directory.EnumerateFiles(absoluteDirectoryPath, "*.*", SearchOption.AllDirectories))
				{
					//we check every filePath against our white list. if it's on there in at least one form we check it.
					foreach (var whiteListItem in fileWhiteList)
					{
						//We have a catch all for a filetype
						if (whiteListItem.StartsWith("*."))
						{
							if (Path.GetExtension(filePath) == whiteListItem.Substring(1))
							{
								//we got a match
								ParseFile(filePath, templateItems);
								break;
							}
						}
						else //a file, like myfile.js
						{
							if (Path.GetFileName(filePath) == whiteListItem)
							{
								//we got a match
								ParseFile(filePath, templateItems);
								break;
							}
						}
					}
					
						
				}
			}

			return templateItems;
		}

		public void ParseFile(string filePath, ConcurrentDictionary<string, TemplateItem> templateItems)
        {
           // Lookup any/all nuggets in the file and for each add a new template item.
			using (var fs = File.OpenText(filePath))
			{
                _nuggetParser.ParseString(fs.ReadToEnd(), delegate(string nuggetString, int pos, Nugget nugget, string i_entity)
                {
				    AddNewTemplateItem(
                        filePath, 
                        i_entity.LineFromPos(pos), 
                        nugget, 
                        templateItems);
                   // Done.
                    return null; // null means we are not modifying the entity.
                });
            }
        }

		private void AddNewTemplateItem(
            string filePath, 
            int lineNumber, 
            Nugget nugget, 
            ConcurrentDictionary<string, TemplateItem> templateItems)
		{
			string reference = filePath + ":" + lineNumber.ToString();
            string msgid = nugget.MsgId.Replace("\r\n", "\n").Replace("\r", "\\n");
                // NB: In memory msgids are normalized so that LFs are converted to "\n" char sequence.
			List<string> tmpList;
           //
            templateItems.AddOrUpdate(
                msgid, 
                // Add routine.
                k => {
			        TemplateItem item = new TemplateItem();
			        item.Id = msgid;

			        tmpList = new List<string>();
			        tmpList.Add(reference);
			        item.References = tmpList;

			        if (nugget.Comment.IsSet()) {
                        tmpList = new List<string>();
                        tmpList.Add(nugget.Comment);
                        item.Comments = tmpList;
                    }

			        return item;
                },
                // Update routine.
                (k, v) => {

					tmpList = v.References.ToList();
					tmpList.Add(reference);
					v.References = tmpList;

			        if (nugget.Comment.IsSet()) {
					    tmpList = v.Comments != null ? v.Comments.ToList() : new List<string>();
					    tmpList.Add(nugget.Comment);
					    v.Comments = tmpList;
                    }

                    return v;
                });
		}
	}
}
