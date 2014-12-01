using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using i18n.Domain.Abstract;
using i18n.Domain.Entities;
using i18n.Helpers;

namespace i18n.Domain.Concrete
{
	public class POTranslationRepository : ITranslationRepository
	{
		private i18nSettings _settings;

		public POTranslationRepository(i18nSettings settings)
		{
			_settings = settings;
		}

		#region load and getters

		public Translation GetTranslation(string langtag)
		{
			return ParseTranslationFile(langtag);
		}


		/// <summary>
		/// Checks in first hand settings file, if not found there it checks file structure
		/// </summary>
		/// <returns>List of available languages</returns>
		public IEnumerable<Language> GetAvailableLanguages()
		{
			//todo: ideally we want to fill the other data in the Language object so this is usable by project incorporating i18n that they can simply lookup available languages. Maybe we even add a country property so that it's easier for projects to add corresponding flags.

			List<string> languages = _settings.AvailableLanguages.ToList();
			Language lang;
			List<Language> dirList = new List<Language>();


			//This means there was no languages from settings
			if (languages.Count == 0
                || (languages.Count == 1 && languages[0] == ""))
			{
				//We instead check for file structure
				DirectoryInfo di = new DirectoryInfo(GetAbsoluteLocaleDir());
				
				foreach (var dir in di.EnumerateDirectories().Select(x => x.Name))
				{
					lang = new Language();
					lang.LanguageShortTag = dir;
					dirList.Add(lang);
				}
			}
			else
			{
				//see if the desired language was one of the returned from settings
				foreach (var language in languages)
				{
					lang = new Language();
					lang.LanguageShortTag = language;
					dirList.Add(lang);
				}
			}

			return dirList;

		}

		/// <summary>
		/// Checks if the language is set as supported in config file
		/// If not it checks if the PO file is available
		/// </summary>
		/// <param name="langtag">The tag for which you want to check if support exists. For instance "sv-SE"</param>
		/// <returns>True if language exists, otherwise false</returns>
		public bool TranslationExists(string langtag)
		{
			List<string> languages = _settings.AvailableLanguages.ToList();

			//This means there was no languages from settings
			if (languages.Count == 0
                || (languages.Count == 1 && languages[0] == ""))
			{
				//We instead check if the file exists
				return File.Exists(GetPathForLanguage(langtag));
			}
			else
			{
				//see if the desired language was one of the returned from settings
				foreach (var language in languages)
				{
					if (language == langtag)
					{
						return true;
					}
				}
			}

			//did not exist in settings nor as file, we return false
			return false;
		}

		public CacheDependency GetCacheDependencyForSingleLanguage(string langtag)
		{
            var path = GetPathForLanguage(langtag);
            if (!File.Exists(path)) {
                return null; }
			return new CacheDependency(path);
		}

		public CacheDependency GetCacheDependencyForAllLanguages()
		{
			return new FsCacheDependency(GetAbsoluteLocaleDir());
		}

		#endregion

		#region save

		/// <summary>
		/// Saves a translation into file with standard pattern locale/langtag/message.po
		/// Also saves a backup of previous version
		/// </summary>
		/// <param name="translation">The translation you wish to save. Must have Language shortag filled out.</param>
		public void SaveTranslation(Translation translation)
		{
			string filePath = GetPathForLanguage(translation.LanguageInformation.LanguageShortTag);
			string backupPath = GetPathForLanguage(translation.LanguageInformation.LanguageShortTag) + ".backup";

			if (File.Exists(filePath)) //we backup one version. more advanced backup solutions could be added here.
			{
				if (File.Exists(backupPath))
				{
					File.Delete(backupPath);
				}
				System.IO.File.Move(filePath, backupPath);
			}

			if (File.Exists(filePath)) //we make sure the old file is removed first
			{
				File.Delete(filePath);
			}

			bool hasReferences = false;

            if (!File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                fileInfo.Create().Close();
            }

			using (StreamWriter stream = new StreamWriter(filePath))
			{
                Console.WriteLine("Writing file: {0}", filePath);
               // Establish ordering of items in PO file.
                var orderedItems = translation.Items.Values
                    .OrderBy(x => x.References == null || x.References.Count() == 0)
                        // Non-orphan items before orphan items.
                    .ThenBy(x => x.MsgKey);
                        // Then order alphanumerically.
               //

				//This is required for poedit to read the files correctly if they contains for instance swedish characters
				stream.WriteLine("msgid \"\"");
				stream.WriteLine("msgstr \"\"");
				stream.WriteLine("\"Content-Type: text/plain; charset=utf-8\\n\"");
				stream.WriteLine();

				foreach (var item in orderedItems)
				{
					hasReferences = false;

					if (item.TranslatorComments != null)
					{
						foreach (var translatorComment in item.TranslatorComments)
						{
							stream.WriteLine("# " + translatorComment);
						}
					}

					if (item.ExtractedComments != null)
					{
						foreach (var extractedComment in item.ExtractedComments)
						{
							stream.WriteLine("#. " + extractedComment);
						}
					}

					if (item.References != null)
					{
						foreach (var reference in item.References)
						{
							hasReferences = true;
							stream.WriteLine("#: " + reference);
						}
					}

					if (item.Flags != null)
					{
						foreach (var flag in item.Flags)
						{
							stream.WriteLine("#, " + flag);
						}
					}

					string prefix = hasReferences ? "" : prefix = "#~ ";

                    if (_settings.MessageContextEnabledFromComment
                        && item.ExtractedComments != null
                        && item.ExtractedComments.Count() != 0) {
                        WriteString(stream, hasReferences, "msgctxt", item.ExtractedComments.First());
                    }

                    WriteString(stream, hasReferences, "msgid", escape(item.MsgId));
                    WriteString(stream, hasReferences, "msgstr", escape(item.Message));

                    stream.WriteLine("");
				}
			}
		}

		/// <summary>
		/// Saves a template file which is a all strings (needing translation) used in the entire project. Not language dependent
		/// </summary>
		/// <param name="items">A list of template items to save. The list should be all template items for the entire project.</param>
		public void SaveTemplate(IDictionary<string, TemplateItem> items)
		{
			string filePath = GetAbsoluteLocaleDir() + "/messages.pot";
			string backupPath = filePath + ".backup";

			if (File.Exists(filePath)) //we backup one version. more advanced backup solutions could be added here.
			{
				if (File.Exists(backupPath))
				{
					File.Delete(backupPath);
				}
				System.IO.File.Move(filePath, backupPath);
			}

			if (File.Exists(filePath)) //we make sure the old file is removed first
			{
				File.Delete(filePath);
			}

		    if (! File.Exists(filePath))
		    {
		        var fileInfo = new FileInfo(filePath);
                var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
		        if (! dirInfo.Exists)
		        {
		            dirInfo.Create();
		        }
		        fileInfo.Create().Close();
		    }

            using (StreamWriter stream = new StreamWriter(filePath))
			{
                Console.WriteLine("Writing file: {0}", filePath);
               // Establish ordering of items in PO file.
                var orderedItems = items.Values
                    .OrderBy(x => x.References == null || x.References.Count() == 0)
                        // Non-orphan items before orphan items.
                    .ThenBy(x => x.MsgKey);
                        // Then order alphanumerically.
               //

				//This is required for poedit to read the files correctly if they contains for instance swedish characters
				stream.WriteLine("msgid \"\"");
				stream.WriteLine("msgstr \"\"");
				stream.WriteLine("\"Content-Type: text/plain; charset=utf-8\\n\"");
				stream.WriteLine();

				foreach (var item in orderedItems)
				{
					if (item.Comments != null)
					{
						foreach (var comment in item.Comments)
						{
							stream.WriteLine("#. " + comment);
						}
					}

					foreach (var reference in item.References)
					{
						stream.WriteLine("#: " + reference);
					}

                    WriteString(stream, true, "msgid", escape(item.MsgId));
                    WriteString(stream, true, "msgstr", ""); // enable loading of POT file into editor e.g. PoEdit.

					stream.WriteLine("");
				}
			}
		}

		#endregion

		#region helpers

		/// <summary>
		/// Gets the locale directory from settings and makes sure it is translated into absolut path
		/// </summary>
		/// <returns>the locale directory in absolute path</returns>
		private string GetAbsoluteLocaleDir()
		{
			return _settings.LocaleDirectory;
		}

		private string GetPathForLanguage(string langtag)
		{
			return Path.Combine(GetAbsoluteLocaleDir(), langtag, "messages.po");
		}

		/// <summary>
		/// Parses a PO file into a Language object
		/// </summary>
		/// <param name="langtag">The language (tag) you wish to load into Translation object</param>
		/// <returns>A complete translation object with all all translations and language values set.</returns>
		private Translation ParseTranslationFile(string langtag)
		{
			//todo: consider that lines we don't understand like headers from poedit and #| should be preserved and outputted again.

			Translation translation = new Translation();
			Language language = new Language();
			language.LanguageShortTag = langtag;
			translation.LanguageInformation = language;
			var items = new ConcurrentDictionary<string, TranslationItem>();

			string path = GetPathForLanguage(langtag);

            if (File.Exists(path)) {
                Console.WriteLine("Reading file: {0}", path);

			    using (var fs = File.OpenText(path))
			    {
				    // http://www.gnu.org/s/hello/manual/gettext/PO-Files.html

				    string line;
				    bool itemStarted = false;
				    while ((line = fs.ReadLine()) != null)
				    {
					    List<string> extractedComments = new List<string>();
					    List<string> translatorComments = new List<string>();
					    List<string> flags = new List<string>();
					    List<string> references = new List<string>();

					    //read all comments, flags and other descriptive items for this string
					    //if we have #~ its a historical/log entry but it is the messageID/message so we skip this do/while
					    if (line.StartsWith("#") && !line.StartsWith("#~"))
					    {
						    do
						    {
							    itemStarted = true;
							    switch (line[1])
							    {
								    case '.': //Extracted comments
									    extractedComments.Add(line.Substring(2).Trim());
									    break;
								    case ':': //references
									    references.Add(line.Substring(2).Trim());
									    break;
								    case ',': //flags
									    flags.Add(line.Substring(2).Trim());
									    break;
								    case '|': //msgid previous-untranslated-string - NOT used by us
									    break;
								    default: //translator comments
									    translatorComments.Add(line.Substring(1).Trim());
									    break;
							    }

						    } while ((line = fs.ReadLine()) != null && line.StartsWith("#"));
					    }

					    if (line != null && (itemStarted || line.StartsWith("#~")))
					    {
						    TranslationItem item = ParseBody(fs, line, extractedComments);
                            if (item != null) {
                               //
					            item.TranslatorComments = translatorComments;
					            item.ExtractedComments = extractedComments;
					            item.Flags = flags;
					            item.References = references;
                               //
                                items.AddOrUpdate(
                                    item.MsgKey, 
                                    // Add routine.
                                    k => {
			                            return item;
                                    },
                                    // Update routine.
                                    (k, v) => {
                                        v.References = v.References.Append(item.References);
                                        v.ExtractedComments = v.ExtractedComments.Append(item.References);
                                        v.TranslatorComments = v.TranslatorComments.Append(item.References);
                                        v.Flags = v.Flags.Append(item.References);
                                        return v;
                                    });
                            }
					    }

					    itemStarted = false;
				    }
			    }
            }
			translation.Items = items;
			return translation;
		}

		/// <summary>
		/// Removes the preceding characters in a file showing that an item is historical/log. That is to say it has been removed from the project. We don't need care about the character as the fact that it lacks references is what tells us it's a log item
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private string RemoveCommentIfHistorical(string line)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				//return null;
                return line;
			}

			if (line.StartsWith("#~"))
			{
				return line.Replace("#~", "").Trim();
			}

			return line;
		}

		/// <summary>
		/// Parses the body of a PO file item. That is to say the message id and the message itself.
		/// Reason for why it must be on second line (textreader) is so that you can read until you have read to far without peek previously for meta data.
		/// </summary>
		/// <param name="fs">A textreader that must be on the second line of a message body</param>
		/// <param name="line">The first line of the message body.</param>
		/// <returns>Returns a TranslationItem with only key, id and message set</returns>
		private TranslationItem ParseBody(TextReader fs, string line, List<string> extractedComments)
		{
            string originalLine = line;

			if (string.IsNullOrEmpty(line)) {
                return null; }

            TranslationItem message = new TranslationItem { MsgKey = "" };
			StringBuilder sb = new StringBuilder();

            string msgctxt = null;
			line = RemoveCommentIfHistorical(line); //so that we read in removed historical records too
			if (line.StartsWith("msgctxt"))
			{
				msgctxt = Unquote(line);
				line = fs.ReadLine();
			}

			line = RemoveCommentIfHistorical(line); //so that we read in removed historical records too
			if (line.StartsWith("msgid"))
			{
				var msgid = Unquote(line);
				sb.Append(msgid);

				while ((line = fs.ReadLine()) != null)
				{
					line = RemoveCommentIfHistorical(line);
                    if (String.IsNullOrEmpty(line))
                    {
                        Console.WriteLine("ERROR - line is empty. Original line: " + originalLine);
                        continue;
                    }
					if (!line.StartsWith("msgstr") && (msgid = Unquote(line)) != null)
					{
						sb.Append(msgid);
					}
					else
					{
						break;
					}
				}

                message.MsgId = Unescape(sb.ToString());
                
                // If no msgctxt is set then msgkey is the msgid; otherwise it is msgid+msgctxt.
                message.MsgKey = string.IsNullOrEmpty(msgctxt) ?
                    message.MsgId:
                    TemplateItem.KeyFromMsgidAndComment(message.MsgId, msgctxt, true);
			}

			sb.Clear();
			line = RemoveCommentIfHistorical(line);
			if (!string.IsNullOrEmpty(line) && line.StartsWith("msgstr"))
			{
				var msgstr = Unquote(line);
				sb.Append(msgstr);

				while ((line = fs.ReadLine()) != null && (msgstr = Unquote(line)) != null)
				{
					line = RemoveCommentIfHistorical(line);
					sb.Append(msgstr);
				}

				message.Message = Unescape(sb.ToString());
			}
            return message;
		}

        /// <summary>
        /// Helper for writing either a msgid or msgstr to the po file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="hasReferences"></param>
        /// <param name="type">"msgid" or "msgstr"</param>
        /// <param name="value"></param>
        private static void WriteString(StreamWriter stream, bool hasReferences, string type, string value)
        {
        // Logic for outputting multi-line msgid.
        //
        // IN : a<LF>b
        // OUT: msgid ""
        //      "a\n"
        //      "b"
        //
        // IN : a<LF>b<LF>
        // OUT: msgid ""
        // OUT: "a\n"
        //      "b\n"
        //
			value = value ?? "";
            value = value.Replace("\r\n", "\n");
            StringBuilder sb = new StringBuilder(100);
           // If multi-line
            if (value.Contains('\n')) {
               // · msgid ""
                sb.AppendFormat("{0} \"\"\r\n", type);
               // · following lines
                sb.Append("\"");
                string s1 = value.Replace("\n", "\\n\"\r\n\"");
                sb.Append(s1);
                sb.Append("\"");
            }
           // If single-line
            else {
                sb.AppendFormat("{0} \"{1}\"", type, value); }
           // If noref...prefix each line with "#~ ".
			if (!hasReferences) {
                sb.Insert(0, "#~ ");
                sb.Replace("\r\n", "\r\n#~ ");
            }
           //
            string s = sb.ToString();
            stream.WriteLine(s);
        }

		#region quoting and escaping

		//this method removes anything before the first quote and also removes first and last quote
		private string Unquote(string lhs, string quotechar = "\"")
		{
			int begin = lhs.IndexOf(quotechar);
			if (begin == -1)
			{
				return null;
			}
			int end = lhs.LastIndexOf(quotechar);
			if (end <= begin)
			{
				return null;
			}
			return lhs.Substring(begin + 1, end - begin - 1);
		}

		private string escape(string s)
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				return null;
			}
			return s.Replace("\"", "\\\"");
		}

		/// <summary>
		/// Looks up in the subject string standard C escape sequences and converts them
		/// to their actual character counterparts.
		/// </summary>
		/// <seealso href="http://stackoverflow.com/questions/6629020/evaluate-escaped-string/8854626#8854626"/>
		private string Unescape(string s)
		{
			Regex regex_unescape = new Regex("\\\\[abfnrtv?\"'\\\\]|\\\\[0-3]?[0-7]{1,2}|\\\\u[0-9a-fA-F]{4}|.", RegexOptions.Singleline);

			StringBuilder sb = new StringBuilder();
			MatchCollection mc = regex_unescape.Matches(s, 0);

			foreach (Match m in mc)
			{
				if (m.Length == 1)
				{
					sb.Append(m.Value);
				}
				else
				{
					if (m.Value[1] >= '0' && m.Value[1] <= '7')
					{
						int i = 0;

						for (int j = 1; j < m.Length; j++)
						{
							i *= 8;
							i += m.Value[j] - '0';
						}

						sb.Append((char)i);
					}
					else if (m.Value[1] == 'u')
					{
						int i = 0;

						for (int j = 2; j < m.Length; j++)
						{
							i *= 16;

							if (m.Value[j] >= '0' && m.Value[j] <= '9')
							{
								i += m.Value[j] - '0';
							}
							else if (m.Value[j] >= 'A' && m.Value[j] <= 'F')
							{
								i += m.Value[j] - 'A' + 10;
							}
							else if (m.Value[j] >= 'a' && m.Value[j] <= 'a')
							{
								i += m.Value[j] - 'a' + 10;
							}
						}

						sb.Append((char)i);
					}
					else
					{
						switch (m.Value[1])
						{
							case 'a':
								sb.Append('\a');
								break;
							case 'b':
								sb.Append('\b');
								break;
							case 'f':
								sb.Append('\f');
								break;
							case 'n':
								sb.Append('\n');
								break;
							case 'r':
								sb.Append('\r');
								break;
							case 't':
								sb.Append('\t');
								break;
							case 'v':
								sb.Append('\v');
								break;
							default:
								sb.Append(m.Value[1]);
								break;
						}
					}
				}
			}

			return sb.ToString();
		}

		#endregion

		#endregion
	}
}
