namespace i18n
{
    /// <summary>
    /// Abstracts a translation service.
    /// </summary>
    public interface ITranslateSvc
    {
        /// <summary>
        /// Returns the translation of the passed string entity which may contain zero or more fully-formed nugget.
        /// </summary>
        /// <param name="entity">
        /// String containing zero or more fully-formed nuggets which are to be translated.
        /// </param>
        /// <returns>
        /// Localized (translated) entity.
        /// </returns>
        string ParseAndTranslate(string entity);
    }
}
