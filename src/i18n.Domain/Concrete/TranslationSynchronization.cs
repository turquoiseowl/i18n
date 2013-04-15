using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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

		public void SynchronizeTranslation(IDictionary<string, TemplateItem> src, Translation dst)
        {
        // Our purpose here is to merge newly parsed message items (src) with those already stored in a translation repo (dst).
        // 1. Where an orphan msgid is found (present in the dst but not the src) we update it in the dst to remove all references.
        // 2. Where a src msgid is missing from dst, we simply ADD it to dst.
        // 3. Where a src msgid is present in dst, we update the item in the dst to match the src (references, comments, etc.).
        //
           // 1.
           // Simply remove all references from dst items, for now.
            foreach (TranslateItem dstItem in dst.Items.Values) {
                dstItem.References = null; }
           // 2. and 3.
            foreach (TemplateItem srcItem in src.Values) {
                TranslateItem dstItem = dst.Items.GetOrAdd(srcItem.Id, k => new TranslateItem { Id = srcItem.Id });
                dstItem.References = srcItem.References;
                dstItem.ExtractedComments = srcItem.Comments;
             }
           // Persist changes.
			_repository.SaveTranslation(dst);
        }

        //MC001 -- TODO -- this method now redundant
		public void SynchronizeTranslationOrg(IDictionary<string, TemplateItem> items, Translation translation)
		{
			//todo: look over desired implementation. Right now it checks for matching id AND matching reference
			//todo: it would be easy to check if translation is updated thereby not saving the file if not updated and thereby sparing us constant checkins to versioning

			bool found = false;
			TranslateItem newItem;

			//step 1 find and update files that exist in both template and and translation and remove references from any translation that is no longer in the template
			foreach (var translationItem in translation.Items)
			{
				found = false;
				foreach (var srcItem in items.Values)
				{
					if (srcItem.Id == translationItem.Value.Id) //we found matching id, now we make sure references match
					{
						foreach (var translationReference in translationItem.Value.References)
						{
							foreach (var templateReference in srcItem.References)
							{
								if (templateReference == translationReference) //we found matching reference
								{
									found = true;
									//we overwrite translation files comments for the ones from template
									translationItem.Value.ExtractedComments = srcItem.Comments; //templates comments comes from code, aka Extracted comments. Translators comments are not in template file
									
									//that is all that is overwritten since everything else such as flags, comments from translator and actual message string has nothing to do with template file
								}
							}
						}
					}
				}
	
				if (!found) //the item no longer exists in the template so we remove the references thus making it log only
				{
					translationItem.Value.References = Enumerable.Empty<string>();
				}
			}


			//step 2 find out if there are any new items in the template and add them to the translation
			foreach (var srcItem in items.Values)
			{
				found = false;
				foreach (var translationItem in translation.Items)
				{

					if (srcItem.Id == translationItem.Value.Id) //we found matching id, now we make sure references match
					{

						foreach (var translationReference in translationItem.Value.References)
						{
							foreach (var templateReference in srcItem.References)
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
					newItem.Id = srcItem.Id;
					newItem.References = srcItem.References;
					newItem.ExtractedComments = srcItem.Comments;

					translation.Items[srcItem.Id] = newItem;
				}
			}

			_repository.SaveTranslation(translation);
		}

		public void SynchronizeAllTranslation(IDictionary<string, TemplateItem> items)
		{
			foreach (var language in _repository.GetAvailableLanguages())
			{
				SynchronizeTranslation(items, _repository.GetLanguage(language.LanguageShortTag));
			}
		}

	}
}
