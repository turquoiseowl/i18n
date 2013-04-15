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

		public void SynchronizeAllTranslation(IDictionary<string, TemplateItem> items)
		{
			foreach (var language in _repository.GetAvailableLanguages())
			{
				SynchronizeTranslation(items, _repository.GetLanguage(language.LanguageShortTag));
			}
		}

	}
}
