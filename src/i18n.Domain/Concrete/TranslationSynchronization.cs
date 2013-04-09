using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using i18n.Domain.Entities;
using i18n.Domain.Abstract;


namespace i18n.Domain.Concrete
{
	public class TranslationSynchronization
	{
		private ITranslationRepository _repository;

		public TranslationSynchronization(ITranslationRepository repository)
		{
			_repository = repository;
		}

		public void SynchronizeTranslation(IEnumerable<TemplateItem> items, Translation translation)
		{
			//todo: look over desired implementation. Right now it checks for matching id AND matching reference
			//todo: it would be easy to check if translation is updated thereby not saving the file if not updated and thereby sparing us constant checkins to versioning

			bool found = false;
			TranslateItem newItem;
			List<TranslateItem> itemList = translation.Items.ToList();

			//step 1 find and update files that exist in both template and and translation and remove references from any translation that is no longer in the template
			foreach (var translationItem in translation.Items)
			{
				found = false;
				foreach (var templateItem in items)
				{
					if (templateItem.Id == translationItem.Id) //we found matching id, now we make sure references match
					{
						foreach (var translationReference in translationItem.References)
						{
							foreach (var templateReference in templateItem.References)
							{
								if (templateReference == translationReference) //we found matching reference
								{
									found = true;
									//we overwrite translation files comments for the ones from template
									translationItem.ExtractedComments = templateItem.Comments; //templates comments comes from code, aka Extracted comments. Translators comments are not in template file
									
									//that is all that is overwritten since everything else such as flags, comments from translator and actual message string has nothing to do with template file
								}
							}
						}
					}
				}
	
				if (!found) //the item no longer exists in the template so we remove the references thus making it log only
				{
					translationItem.References = Enumerable.Empty<string>();
				}
			}


			//step 2 find out if there are any new items in the template and add them to the translation
			foreach (var templateItem in items)
			{
				found = false;
				foreach (var translationItem in translation.Items)
				{
					if (templateItem.Id == translationItem.Id) //we found matching id, now we make sure references match
					{
						foreach (var translationReference in translationItem.References)
						{
							foreach (var templateReference in templateItem.References)
							{
								if (templateReference == translationReference) //we found matching reference
								{
									found = true;

									//we found a match which means this template item already excisted in the translation so we right away want to go to next template item
									break;
								}
							}
							if (found)
							{
								break;
							}
						}
						if (found)
						{
							break;
						}
					}
					if (found)
					{
						break;
					}
				}
	
				if (!found) //the template item did not excist in the translation so we will create it.
				{
					newItem = new TranslateItem();
					newItem.Id = templateItem.Id;
					newItem.References = templateItem.References;
					newItem.ExtractedComments = templateItem.Comments;

					itemList.Add(newItem);
				}
			}

			translation.Items = itemList;
			_repository.SaveTranslation(translation);
		}

		public void SynchronizeAllTranslation(IEnumerable<TemplateItem> items)
		{
			foreach (var language in _repository.GetAvailableLanguages())
			{
				SynchronizeTranslation(items, _repository.GetLanguage(language.LanguageShortTag));
			}
		}

	}
}
