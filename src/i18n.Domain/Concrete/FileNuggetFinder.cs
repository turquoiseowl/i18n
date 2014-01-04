using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using i18n.Domain.Abstract;
using i18n.Domain.Entities;
using i18n.Helpers;

namespace i18n.Domain.Concrete
{
	public class FileNuggetFinder : INuggetFinder
	{
		private i18nSettings _settings;

        private NuggetParser _nuggetParser;

		public FileNuggetFinder(i18nSettings settings)
		{
			_settings = settings;
            _nuggetParser = new NuggetParser(new NuggetTokens(
			    _settings.NuggetBeginToken,
			    _settings.NuggetEndToken,
			    _settings.NuggetDelimiterToken,
			    _settings.NuggetCommentToken),
                NuggetParser.Context.SourceProcessing);
		}

		/// <summary>
		/// Goes through the Directories to scan recursively and starts a scan of each while that matches the whitelist. (both from settings)
		/// </summary>
		/// <returns>All found nuggets.</returns>
		public IDictionary<string, TemplateItem> ParseAll()
		{
			IEnumerable<string> fileWhiteList = _settings.WhiteList;
			IEnumerable<string> directoriesToSearchRecursively = _settings.DirectoriesToScan;

			string currentFullPath;
			bool blacklistFound = false;

			var templateItems = new ConcurrentDictionary<string, TemplateItem>();
                // Collection of template items keyed by their id.

			foreach (var directoryPath in directoriesToSearchRecursively)
			{
				foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories))
				{
					blacklistFound = false;
                    try
                    {
                        currentFullPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
                        foreach (var blackItem in _settings.BlackList)
                        {
                            if (currentFullPath == null || currentFullPath.StartsWith(blackItem, StringComparison.OrdinalIgnoreCase))
                            {
                                //this is a file that is under a blacklisted directory so we do not parse it.
                                blacklistFound = true;
                                break;
                            }
                        }
                        if (!blacklistFound)
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
                    catch (PathTooLongException ex)
                    {
                        Console.WriteLine("Skipping path that was too long: " + filePath);
                    }
				}
			}

			return templateItems;
		}

		private void ParseFile(string filePath, ConcurrentDictionary<string, TemplateItem> templateItems)
        {
            DebugHelpers.WriteLine("FileNuggetFinder.ParseFile -- {0}", filePath);
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
            string key = TemplateItem.KeyFromMsgidAndComment(msgid, nugget.Comment, _settings.MessageContextEnabledFromComment);
			List<string> tmpList;
           //
            templateItems.AddOrUpdate(
                key,
                // Add routine.
                k => {
			        TemplateItem item = new TemplateItem();
                    item.MsgKey = key;
			        item.MsgId = msgid;

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
