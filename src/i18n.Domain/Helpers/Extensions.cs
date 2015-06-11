using System;
using System.Text;
using System.Text.RegularExpressions;

namespace i18n
{
    internal static class Extensions
    {
        /// <summary>
        /// String extension method to simplify testing for non-null/non-empty values.
        /// </summary>
        public static bool IsSet(
            this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Prepends a folder to the path part of the passed URL string.
        /// </summary>
        /// <param name="url">Either an absolute or relative URL string.</param>
        /// <param name="folder">Folder part to be prepended. E.g. "account".</param>
        /// <returns>Amended URL string.</returns>
        /// <remarks>
        /// Examples:
        /// <para>
        /// http://example.com , en -> http://example.com/en
        /// http://example.com/ , en -> http://example.com/en
        /// http://example.com/accounts , en -> http://example.com/en/accounts
        /// / , en -> /en
        /// </para>
        /// </remarks>
        public static string UrlPrependPath(this string url, string folder)
        {
            if (!folder.IsSet()) {
                return url; }

            // If absolute url (include host and optionally scheme)
            // NOTE: The check for .IsFile is meant for Mono systems where "/my-path/" is
            //       a valid absolute path. It'll be resolved to "file:///my-path/".
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri) && !uri.IsFile) {
                UriBuilder ub = new UriBuilder(url);
                ub.Path = ub.Path.UrlPrependPath(folder);
                return ub.Uri.ToString(); // Go via Uri to avoid port 80 being added.
            }
            
            // Url is relative.
            StringBuilder sb = new StringBuilder(url.Length + folder.Length + 10);
            if (folder[0] != '/') {
                sb.Append("/"); }
            sb.Append(folder);
            if (url.IsSet() && url != "/") {
                if (url[0] != '/') {
                    sb.Append("/"); }
                sb.Append(url);
            }
            return sb.ToString();
        }

    }
}
