using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using i18n.Domain.Entities;

namespace i18n.Domain.Concrete
{
	public class NuggetFileParser
	{
		private i18nSettings _settings;

		public NuggetFileParser(i18nSettings settings)
		{
			_settings = settings;
		}

		public IEnumerable<TemplateItem> ParseAll()
		{
			IEnumerable<string> fileWhiteList = _settings.WhiteList;
			IEnumerable<string> directoriesToSearchRecursively = _settings.DirectoriesToScan;

			List<TemplateItem> templateList = new List<TemplateItem>();
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


				foreach (string file in Directory.EnumerateFiles(absoluteDirectoryPath, "*.*", SearchOption.AllDirectories))
				{
					//we check every file against our white list. if it's on there in at least one form we check it.
					foreach (var whiteListItem in fileWhiteList)
					{
						//We have a catch all for a filetype
						if (whiteListItem.StartsWith("*."))
						{
							if (Path.GetExtension(file) == whiteListItem.Substring(1))
							{
								//we got a match
								templateList.AddRange(ParseFile(file));
								break;
							}
						}
						else //a file, like myfile.js
						{
							if (Path.GetFileName(file) == whiteListItem)
							{
								//we got a match
								templateList.AddRange(ParseFile(file));
								break;
							}
						}
					}
					
						
				}
			}


			return templateList;
		}


		//todo: see comment in TranslateItem.cs about how line breaks should be handled, this searcher also needs to be updated.
		//todo: think about being able to escape the different delimiters so they can exist in the text
		//todo: this function is simple and should probably be refactored, it does not in any way support multiline
		//todo: could also be an idea to check if the line is a code comment, altho comments looks different in different file types
		public IEnumerable<TemplateItem> ParseFile(string file)
		{
			List<TemplateItem> list = new List<TemplateItem>();
			string startToken = _settings.NuggetBeginToken;
			string endToken = _settings.NuggetEndToken;
			string delimiterToken = _settings.NuggetDelimiterToken;

			int currentCharecterIndex = 0;
			int startedIndex = -1;
			int endIndex = -1;
			string nugget = "";
			string line;
			bool haveOpened = false;
			int lineNumber = 0;

			int delimiterIndex;
			int endTokenIndex;

			using (var fs = File.OpenText(file))
			{
				while ((line = fs.ReadLine()) != null)
				{
					currentCharecterIndex = 0;
					
					lineNumber++;

					//will find all starts of nuggets, inside we need to make sure to forward the index if we find an end
					while ((currentCharecterIndex = line.IndexOf(startToken, currentCharecterIndex)) != -1)
					{
						haveOpened = true;
						startedIndex = currentCharecterIndex;

						delimiterIndex = line.IndexOf(delimiterToken, currentCharecterIndex);
						endTokenIndex = line.IndexOf(endToken, currentCharecterIndex);

						//this means we found an end to the nugget itself, parameters follow but we are not interessted
						if (delimiterIndex != -1 && delimiterIndex < endTokenIndex)
						{
							endIndex = delimiterIndex;
						}
						else if (endTokenIndex != -1)
						{
							endIndex = endTokenIndex;
						}

						if (endIndex != -1)
						{
							nugget = line.Substring(startedIndex + startToken.Length, endIndex - startedIndex - startToken.Length);
							AddNewTemplateItem(file, lineNumber, nugget, list);
						}

						currentCharecterIndex = endIndex;
						haveOpened = false;
						startedIndex = -1;
						endIndex = -1;
						nugget = "";

						//we never found an end to our nugget, so we break out of this line to go to next
						if (currentCharecterIndex == -1)
						{
							break;
						}

					}
				}
			}
			

			return list;
		}

		private void AddNewTemplateItem(string file, int lineNumber, string itemString, List<TemplateItem> existing)
		{
			string reference = file + ":" + lineNumber.ToString();
			List<string> tmpList;

			foreach (var templateItem in existing)
			{
				//this string already exist so we simple add a reference
				if (templateItem.Id == itemString)
				{
					tmpList = templateItem.References.ToList();
					tmpList.Add(reference);
					templateItem.References = tmpList;
					return;
				}
			}

			//if we got this far we simply add the item
			TemplateItem item = new TemplateItem();

			item.Id = itemString;
			tmpList = new List<string>();
			tmpList.Add(reference);
			item.References = tmpList;
			existing.Add(item);

		}
	}
}
