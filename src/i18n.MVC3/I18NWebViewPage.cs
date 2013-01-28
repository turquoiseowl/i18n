using System.Web;
using System.Web.Mvc;

namespace i18n
{
    /// <summary>
    /// A base view providing an alias for localizable resources
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class I18NWebViewPage<T> : WebViewPage<T>, ILocalizing
    {
        private readonly I18NSession _session;

        protected I18NWebViewPage()
        {
            _session = new I18NSession();
        }

        public IHtmlString _(string text)
        {
            return new MvcHtmlString(_session.GetText(Context, text));
        }

        /// <summary>
        /// Returns markup where the passed value string is wrapped with double quotes,
        /// with optional support for generating an HTML attribute="value" combination.
        /// </summary>
        /// <remarks>
        /// Workaround for limitation writing HTML attributes whose values are translatable strings,
        /// as discussed in issue #8.
        /// The problem is that xgettext, when set to c# mode, does not detect the _() function when
        /// used within double quotes. Thus, &lt;img alt="@_("logo")"&gt; doesn't get picked up.
        /// Using this overload of the _() helper avoids the problem by encoding the quotes within
        /// the method.
        /// </remarks>
        /// <param name="value">String which is to be wrapped in double quotes and also translatable
        /// (and picked up by xgettext and so exported to the POT file).</param>
        /// <param name="attrname">Optional name of an HTML attribute when producing markup for an HTML attribute e.g. alt.
        /// Null or empty string if just the quoted value string to be output.</param>
        /// <returns>Raw html markup string of the form "value" or attrname="value".</returns>
        /// <example>
        /// The following Razor HTML syntax:
        ///       &lt;img @_("Our logo", "alt") src="..."&gt;...&lt;/img&gt;
        /// will output as:
        ///       &lt;img alt="Our logo" src="..."&gt;...&lt;/img&gt;
        /// </example>
        /// <example>
        /// The following Razor Javascript syntax:
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert(@_("Hello there", ""));
        ///           });
        ///     &lt;/script&gt;
        /// will output as:
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert("Hello there");
        ///           });
        ///     &lt;/script&gt;
        /// </example>
        /// <seealso href="https://github.com/danielcrenna/i18n/issues/8"/>
        public IHtmlString _(string value, string attrname)
        {
            value = _session.GetText(Context, value);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value):
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        }    

        public string __(string text)
        {
            return _session.GetText(Context, text);
        }    
    }

    
    /// <summary>
    /// A base view providing an alias for localizable resources
    /// </summary>
    public abstract class I18NWebViewPage : WebViewPage, ILocalizing
    {
        private readonly I18NSession _session;

        protected I18NWebViewPage()
        {
            _session = new I18NSession();
        }

        public IHtmlString _(string text)
        {
            return new MvcHtmlString(_session.GetText(Context, text));
        }
 
        /// <summary>
        /// Returns markup where the passed value string is wrapped with double quotes,
        /// with optional support for generating an HTML attribute="value" combination.
        /// </summary>
        /// <remarks>
        /// Workaround for limitation writing HTML attributes whose values are translatable strings,
        /// as discussed in issue #8.
        /// The problem is that xgettext, when set to c# mode, does not detect the _() function when
        /// used within double quotes. Thus, &lt;img alt="@_("logo")"&gt; doesn't get picked up.
        /// Using this overload of the _() helper avoids the problem by encoding the quotes within
        /// the method.
        /// <example>
        /// The following Razor HTML syntax:
        ///       &lt;img @_("Our logo", "alt") src="..."&gt;...&lt;/img&gt;
        /// will output as:
        ///       &lt;img alt="Our logo" src="..."&gt;...&lt;/img&gt;
        /// </example>
        /// <example>
        /// The following Razor Javascript syntax:
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert(@_("Hello there", ""));
        ///           });
        ///     &lt;/script&gt;
        /// will output as:
        ///     &lt;script type="text/javascript"&gt;
        ///           $(function () {
        ///             alert("Hello there");
        ///           });
        ///     &lt;/script&gt;
        /// </example>
        /// </remarks>
        /// <param name="value">String which is to be wrapped in double quotes and also translatable
        /// (and picked up by xgettext and so exported to the POT file).</param>
        /// <param name="attrname">Optional name of an HTML attribute when producing markup for an HTML attribute e.g. alt.
        /// Null or empty string if just the quoted value string to be output.</param>
        /// <returns>Raw html markup string of the form "value" or attrname="value".</returns>
        /// <seealso href="https://github.com/danielcrenna/i18n/issues/8"/>
        public IHtmlString _(string value, string attrname)
        {
            value = _session.GetText(Context, value);
            string raw = string.IsNullOrEmpty(attrname) ?
                string.Format("\"{0}\"", value):
                string.Format("{0}=\"{1}\"", attrname, value);
            return new System.Web.HtmlString(raw);
        }    

        public string __(string text)
        {
            return _session.GetText(Context, text);
        }    
   }
}