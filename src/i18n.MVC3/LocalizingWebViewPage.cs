using System.Web;
using System.Web.Mvc;

namespace i18n
{
    using i18n.NamedStringFormater;

    /// <summary>
    /// A base view providing an alias for localizable resources
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LocalizingWebViewPage<T> : WebViewPage<T>, ILocalizing
    {

    #region [ILocalizing]

        /// <summary>
        /// Looks up and returns any translation available of the given text.
        /// </summary>
        /// <param name="text">The text to localize.</param>
        /// <returns>
        /// Either a translation of the text or if none found, returns text as is.
        /// </returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// </remarks>
        public IHtmlString _(string text)
        {
            return new HtmlString(Context.GetText(text));
        }
        public IHtmlString _(string text, params object[] parameters)
        {
            return new HtmlString(string.Format(Context.GetText(text), parameters));
        }

        public IHtmlString _(string text, object source)
        {
            return new HtmlString(Context.GetText(text).Format(source));
        }

    #endregion

        /// <summary>
        /// Returns markup where the passed value string is wrapped with double quotes,
        /// with optional support for generating an HTML attribute="value" combination.
        /// </summary>
        /// <param name="value">String which is to be wrapped in double quotes and also translatable
        /// (and picked up by xgettext and so exported to the POT file).</param>
        /// <param name="attrname">Optional name of an HTML attribute when producing markup for an HTML attribute e.g. alt.
        /// Null or empty string if just the quoted value string to be output.</param>
        /// <returns>Raw html markup string of the form "value" or attrname="value".</returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// <para>
        /// Workaround for limitation writing HTML attributes whose values are translatable strings,
        /// as discussed in issue #8.
        /// </para>
        /// <para>
        /// The problem is that xgettext, when set to c# mode, does not detect the _() function when
        /// used within double quotes. Thus, &lt;img alt="@_("logo")"&gt; doesn't get picked up.
        /// </para>
        /// <para>
        /// Using this overload of the _() helper avoids the problem by encoding the quotes within
        /// the method.
        /// </para>
        /// <para>
        /// The following Razor HTML syntax:
        /// </para>
        /// <para>
        ///       &lt;img @_("Our logo", "alt") src="..."&gt;...&lt;/img&gt;
        /// </para>
        /// <para>
        /// will output as:
        /// </para>
        /// <para>
        ///       &lt;img alt="Our logo" src="..."&gt;...&lt;/img&gt;
        /// </para>
        /// <para>
        /// The following Razor Javascript syntax:
        /// </para>
        /// <para>
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert(@_("Hello there", ""));
        ///           });
        ///     &lt;/script&gt;
        /// </para>
        /// <para>
        /// will output as:
        /// </para>
        /// <para>
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert("Hello there");
        ///           });
        ///     &lt;/script&gt;
        /// </para>
        /// </remarks>
        /// <seealso href="https://github.com/danielcrenna/i18n/issues/8"/>
        public IHtmlString _(string value, string attrname)
        {
            value = Context.GetText(value);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value):
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        }    
        public IHtmlString _(string value, string attrname, params object[] parameters)
        {
            value = string.Format(Context.GetText(value), parameters);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value):
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        }

        public IHtmlString _(string value, string attrname, object source)
        {
            value = Context.GetText(value).Format(source);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value) :
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        }  

        /// <summary>
        /// Looks up and returns a plain string containing any translation available of the given text.
        /// </summary>
        /// <param name="text">The text to localize.</param>
        /// <returns>
        /// Plain string containing either a translation of the text or if none found, the text as is.
        /// </returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// </remarks>
        public string __(string text)
        {
            return Context.GetText(text);
        }    
        public string __(string text, params object[] parameters)
        {
            return string.Format(Context.GetText(text), parameters);
        }

        public string __(string text, object source)
        {
            return Context.GetText(text).Format(source);
        }
    }

    
    /// <summary>
    /// A base view providing an alias for localizable resources
    /// </summary>
    public abstract class LocalizingWebViewPage : WebViewPage, ILocalizing
    {

    #region [ILocalizing]

        /// <summary>
        /// Looks up and returns any translation available of the given text.
        /// </summary>
        /// <param name="text">The text to localize.</param>
        /// <returns>
        /// Either a translation of the text or if none found, returns text as is.
        /// </returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// </remarks>
        public IHtmlString _(string text)
        {
            return new HtmlString(Context.GetText(text));
        }
        public IHtmlString _(string text, params object[] parameters)
        {
            return new HtmlString(string.Format(Context.GetText(text), parameters));
        }

        public IHtmlString _(string text, object source)
        {
            return new HtmlString(Context.GetText(text).Format(source));
        }

    #endregion

        /// <summary>
        /// Returns markup where the passed value string is wrapped with double quotes,
        /// with optional support for generating an HTML attribute="value" combination.
        /// </summary>
        /// <param name="value">String which is to be wrapped in double quotes and also translatable
        /// (and picked up by xgettext and so exported to the POT file).</param>
        /// <param name="attrname">Optional name of an HTML attribute when producing markup for an HTML attribute e.g. alt.
        /// Null or empty string if just the quoted value string to be output.</param>
        /// <returns>Raw html markup string of the form "value" or attrname="value".</returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// <para>
        /// Workaround for limitation writing HTML attributes whose values are translatable strings,
        /// as discussed in issue #8.
        /// </para>
        /// <para>
        /// The problem is that xgettext, when set to c# mode, does not detect the _() function when
        /// used within double quotes. Thus, &lt;img alt="@_("logo")"&gt; doesn't get picked up.
        /// </para>
        /// <para>
        /// Using this overload of the _() helper avoids the problem by encoding the quotes within
        /// the method.
        /// </para>
        /// <para>
        /// The following Razor HTML syntax:
        /// </para>
        /// <para>
        ///       &lt;img @_("Our logo", "alt") src="..."&gt;...&lt;/img&gt;
        /// </para>
        /// <para>
        /// will output as:
        /// </para>
        /// <para>
        ///       &lt;img alt="Our logo" src="..."&gt;...&lt;/img&gt;
        /// </para>
        /// <para>
        /// The following Razor Javascript syntax:
        /// </para>
        /// <para>
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert(@_("Hello there", ""));
        ///           });
        ///     &lt;/script&gt;
        /// </para>
        /// <para>
        /// will output as:
        /// </para>
        /// <para>
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert("Hello there");
        ///           });
        ///     &lt;/script&gt;
        /// </para>
        /// </remarks>
        /// <seealso href="https://github.com/danielcrenna/i18n/issues/8"/>
        public IHtmlString _(string value, string attrname)
        {
            value = Context.GetText(value);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value):
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        }    
        public IHtmlString _(string value, string attrname, params object[] parameters)
        {
            value = string.Format(Context.GetText(value), parameters);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value):
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        }

        public IHtmlString _(string value, string attrname, object source)
        {
            value = Context.GetText(value).Format(source);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value) :
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        } 

        /// <summary>
        /// Looks up and returns a plain string containing any translation available of the given text.
        /// </summary>
        /// <param name="text">The text to localize.</param>
        /// <returns>
        /// Plain string containing either a translation of the text or if none found, the text as is.
        /// </returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// </remarks>
        public string __(string text)
        {
            return Context.GetText(text);
        }    
        public string __(string text, params object[] parameters)
        {
            return string.Format(Context.GetText(text), parameters);
        }

        public string __(string text, object source)
        {
            return Context.GetText(text).Format(source);
        }
    }
}
