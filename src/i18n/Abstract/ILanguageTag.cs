using System;
using System.Globalization;

namespace i18n
{
    /// <summary>
    /// Describes a language tag value, as defined in RFC 5646 (BCP 47).
    /// </summary>
    /// <seealso href="http://tools.ietf.org/html/rfc5646#section-2.1"/>
    public interface ILanguageTag : IEquatable<ILanguageTag>
    {
        /// <summary>
        /// Returns the full language tag.
        /// </summary>
        /// <returns>A string describing the full language tag or "" if null or error instance.</returns>
        string ToString();

        /// <summary>
        /// Returns the mandatory 2 character language subtag.
        /// </summary>
        /// <returns>A string describing a language subtag, or null if no such subtag present (indicating a null or error instance).</returns>
        string GetLanguage();

        /// <summary>
        /// Returns the optional 3 character extlang subtag.
        /// </summary>
        /// <returns>A string describing a extlang subtag, or null if no such subtag present.</returns>
        string GetExtlang();

        /// <summary>
        /// Returns the optional 4 character script subtag.
        /// </summary>
        /// <returns>A string describing a script subtag, or null if no such subtag present.</returns>
        string GetScript();

        /// <summary>
        /// Returns the optional 2 or 3 character region subtag.
        /// </summary>
        /// <returns>A string describing a region subtag, or null if no such subtag present.</returns>
        string GetRegion();

        /// <summary>
        /// Returns zero or more optional variant subtags.
        /// </summary>
        /// <returns>An array of strings describing the variant subtags, or null if no such subtag present.</returns>
        string[] GetVariant();

        /// <summary>
        /// Returns the optional extension subtag.
        /// </summary>
        /// <returns>A string describing an extension subtag, or null if no such subtag present.</returns>
        string GetExtension();

        /// <summary>
        /// Returns the optional privateuse subtag.
        /// </summary>
        /// <returns>A string describing a privateuse subtag, or null if no such subtag present.</returns>
        string GetPrivateuse();

        /// <summary>
        /// Returns an object representing any logical parent of the tag.
        /// </summary>
        /// <returns>Parent object or null if no parent.</returns>
        ILanguageTag GetParent();

        /// <summary>
        /// Returns the maximum number of parents possible.
        /// This is really a static value provided by the underlying impl.
        /// </summary>
        int GetMaxParents();

        /// <summary>
        /// Corresponding CultureInfo instance, or null if the langtag is unsupported on this system.
        /// </summary>
        CultureInfo GetCultureInfo();

        /// <summary>
        /// If the system supports a cultureinfo object for the language, this is the native name of 
        /// the language suitable for user display, otherwise it is the language tag string.
        /// </summary>
        string GetNativeNameTitleCase();
    }
}
