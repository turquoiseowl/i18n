using System;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace i18n
{

/*
    ResponseFilterModule can be installed like this:

        IIS7+ Integrated mode:

          <system.webServer>
            <modules>
              <add name="i18nResponseFilter" type="i18n.ResponseFilterModule, i18n" />
            </modules>
          </system.webServer>

        IIS7 Classic mode and II6:

          <system.web>
            <httpModules>
              <add name="i18nResponseFilter" type="i18n.ResponseFilterModule, i18n" /> <!-- #37 -->
            </httpModules>
          </system.web>
*/

    /// <summary>
    /// HTTP module responsible for installing ResponseFilter into the ASP.NET pipeline.
    /// </summary>
    public class ResponseFilterModule : IHttpModule
    {

    // Implementation

        /// <summary>
        /// Regular expression that controls the ContextTypes elligible for localization
        /// by the filter.
        /// </summary>
        /// <remarks>
        /// Defaults to text/html and application/javascript.
        /// Client may customise this member, for instance in Application_Start.
        /// </remarks>
        public static Regex m_regex_contenttypes = new Regex("^(?:text/html|application/javascript)$");

        protected static void RedirectWithLanguage(HttpContextBase context, string langtag)
        {
            // Construct localized URL.
            string urlNew = LocalizedApplication.UrlLocalizer.SetLangTagInUrlPath(context.Request.RawUrl, langtag);

            // Redirect user agent to new local URL.
            if (LocalizedApplication.PermanentRedirects) {
                context.Response.StatusCode = 301;
                context.Response.Status = "301 Moved Permanently";
            }
            else {
                context.Response.StatusCode = 302;
                context.Response.Status = "302 Moved Temporarily";
            }
            context.Response.RedirectLocation = urlNew;
            context.Response.End();
        }

        /// <summary>
        /// Implements the Early Url Localization logic.
        /// </summary>
        protected static void EarlyUrlLocalization(HttpContextBase context)
        {
        // https://docs.google.com/drawings/d/1cH3_PRAFHDz7N41l8Uz7hOIRGpmgaIlJe0fYSIOSZ_Y/edit?usp=sharing
        //
            LanguageTag lt = null;
            string urlNonlocalized;
            string langtag = LocalizedApplication.UrlLocalizer.ExtractLangTagFromUrl(context.Request.RawUrl, out urlNonlocalized);

            // Is URL localized?
            if (langtag == null)
            {
                // NO.
                // langtag = best match between
                // 1. Inferred user languages (cookie and Accept-Language header)
                // 2. App Languages.
                HttpCookie cookie_langtag = context.Request.Cookies.Get("i18n.langtag");
                if (cookie_langtag != null) {
                    lt = LanguageHelpers.GetMatchingAppLanguage(cookie_langtag.Value); }
                if (lt == null) {
                    lt = LanguageHelpers.GetMatchingAppLanguage(context.GetRequestUserLanguages()); }
                if (lt == null) {
                    throw new InvalidOperationException("Expected GetRequestUserLanguages to fall back to default language."); }

                // Redirect user agent to localized URL.
                RedirectWithLanguage(context, lt.ToString());
                return;
            }
            // YES. Does langtag EXACTLY match an App Language?
            LanguageTag appLangTag = LanguageHelpers.GetMatchingAppLanguage(langtag);
            if (appLangTag.IsValid()
                && appLangTag.Equals(langtag))
            {
                // YES. Establish langtag as the PAL for the request.
                context.SetPrincipalAppLanguageForRequest(appLangTag);

                // Rewrite URL for this request.
                context.RewritePath(urlNonlocalized);

                // Continue handling request.
                return;
            }
            // NO. Does langtag LOOSELY match an App Language?
            else if (appLangTag.IsValid()
                && !appLangTag.Equals(langtag))
            {
                // YES. Localize URL with matching App Language.
                // Redirect user agent to localized URL.
                RedirectWithLanguage(context, appLangTag.ToString());
                return;
            }
            // NO. Do nothing to URL; expect a 404 which corresponds to language not supported.
            // Continue handling request.
        }

    // Events handlers

        private void OnBeginRequest(object sender, EventArgs e)
        {
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("ResponseFilterModule::OnBeginRequest -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            EarlyUrlLocalization(context);
        }

        private void OnReleaseRequestState(object sender, EventArgs e)
        {
        // We get here once per request late on in the pipeline but prior to the response being filtered.
        // We take the opportunity to insert our filter in order to intercept that.
        //
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("ResponseFilterModule::OnReleaseRequestState -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            // If the content type of the entity is eligible for processing...wire up our filter
            // to do the processing. The entity data will be run through the filter a bit later on
            // in the pipeline.
            if (m_regex_contenttypes.Match(context.Response.ContentType).Success) {
                DebugHelpers.WriteLine("ResponseFilterModule::OnReleaseRequestState -- Installing filter");
                context.Response.Filter = new ResponseFilter(context, context.Response.Filter);
            }
        }

        private void OnEndRequest(object sender, EventArgs e)
        {
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("ResponseFilterModule::OnEndRequest -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);
        }

    #region [IHttpModule]

        public void Init(HttpApplication application)
        {
            DebugHelpers.WriteLine("ResponseFilterModule::Init -- application: {0}", application);
            
            // Wire up our event handlers into the ASP.NET pipeline.
            application.BeginRequest += OnBeginRequest;
            application.ReleaseRequestState += OnReleaseRequestState;
            //application.EndRequest += OnEndRequest;
        }

        public void Dispose() {}

    #endregion

    }

    public class ResponseFilter : Stream
    {
        /// <summary>
        /// Regex for finding and replacing msgid nuggets.
        /// </summary>
        protected static readonly Regex m_regex_nugget = new Regex(
            //@"«««(.+?)»»»", 
            @"\[\[\[(.+?)\]\]\]", 
            RegexOptions.CultureInvariant);
            // [[[
            //      Match opening sequence.
            // .+?
            //      Lazily match chars up to
            // ]]]
            //      ... closing sequence
        protected static readonly Regex m_regex_script = new Regex(
            "(?<pre><script[.\\s]*?src\\s*=\\s*[\"']\\s*)(?<url>.+?)(?<post>\\s*[\"'][^>]*?>)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                // Notes:
                //      \s = whitespace

        /// <summary>
        /// The stream onto which we pass data once processed.
        /// </summary>
        protected Stream m_outputStream;

        /// <summary>
        /// HTTP context with which the filter is associated.
        /// </summary>
        protected HttpContextBase m_httpContext;

        public ResponseFilter(HttpContextBase httpContext, Stream outputStream)
        {
            m_httpContext = httpContext;
            m_outputStream = outputStream;
        }

    #region [Stream]

        public override void Write(byte[] buffer, int offset, int count)
        {
            DebugHelpers.WriteLine("ResponseFilter::Write -- count: {0}", count);

            // Get a string out of input bytes.
            Encoding enc = m_httpContext.Response.ContentEncoding;
            string entity = enc.GetString(buffer, offset, count);

            // Translate any embedded messages.
            entity = ProcessNuggets(entity, m_httpContext);

            // Patch all URLs in the entity which are:
            // 1. local (non-remote)
            // 2. are not already localized
            //#37 TODO: Extend this to path all local (non-remote) URLs in the entity.
            // This will save a redirect on subsequent requests.
            // <img src="..."> tags
            // <a href="..."> tags
            // <link href="..."> tags
            // Test embedded tags e.g. <a src="..."><img  src="..."/><a> etc.
            entity = PatchScriptUrls(entity, m_httpContext.GetPrincipalAppLanguageForRequest().ToString());

            //DebugHelpers.WriteLine("ResponseFilter::Write -- entity:\n{0}", entity);

            // Render the string back to an array of bytes.
            buffer = enc.GetBytes(entity);
            count = buffer.Length;

            // Forward data on to the original response stream.
            m_outputStream.Write(buffer, 0, count);
        }

        public override bool CanRead  { get { DebugHelpers.WriteLine("ResponseFilter::CanRead::get"); return m_outputStream.CanRead; } }
        public override bool CanSeek  { get { DebugHelpers.WriteLine("ResponseFilter::CanSeek::get"); return m_outputStream.CanSeek; } }
        public override bool CanWrite { get { DebugHelpers.WriteLine("ResponseFilter::CanWrite::get"); return m_outputStream.CanWrite; } }
        public override long Length   { get { DebugHelpers.WriteLine("ResponseFilter::Length::get"); return m_outputStream.Length; } }
        public override long Position { get { DebugHelpers.WriteLine("ResponseFilter::Position::get"); return m_outputStream.Position; } set { DebugHelpers.WriteLine("ResponseFilter::Position::set"); m_outputStream.Position = value; } }
        public override void Flush()  { DebugHelpers.WriteLine("ResponseFilter::Flush"); m_outputStream.Flush(); }
        public override long Seek(long offset, SeekOrigin origin) { DebugHelpers.WriteLine("ResponseFilter::Seek"); return m_outputStream.Seek(offset, origin); }
        public override void SetLength(long value) { DebugHelpers.WriteLine("ResponseFilter::SetLength"); m_outputStream.SetLength(value); }
        public override int Read(byte[] buffer, int offset, int count) { DebugHelpers.WriteLine("ResponseFilter::Read"); return m_outputStream.Read(buffer, offset, count); }

        public override bool CanTimeout  { get { DebugHelpers.WriteLine("ResponseFilter::CanTimeout::get"); return m_outputStream.CanTimeout; } }
        public override int ReadTimeout { get { DebugHelpers.WriteLine("ResponseFilter::ReadTimeout::get"); return m_outputStream.ReadTimeout; } set { DebugHelpers.WriteLine("ResponseFilter::ReadTimeout::set"); m_outputStream.ReadTimeout = value; } }
        public override int WriteTimeout { get { DebugHelpers.WriteLine("ResponseFilter::WriteTimeout::get"); return m_outputStream.WriteTimeout; } set { DebugHelpers.WriteLine("ResponseFilter::WriteTimeout::set"); m_outputStream.WriteTimeout = value; } }
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) { DebugHelpers.WriteLine("ResponseFilter::BeginRead"); return m_outputStream.BeginRead(buffer, offset, count, callback, state); }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) { DebugHelpers.WriteLine("ResponseFilter::BeginWrite"); return m_outputStream.BeginWrite(buffer, offset, count, callback, state); }
        public override void Close() { DebugHelpers.WriteLine("ResponseFilter::Close"); m_outputStream.Close(); }
        public override int EndRead(IAsyncResult asyncResult) { DebugHelpers.WriteLine("ResponseFilter::EndRead"); return m_outputStream.EndRead(asyncResult); }
        public override void EndWrite(IAsyncResult asyncResult) { DebugHelpers.WriteLine("ResponseFilter::EndWrite"); m_outputStream.EndWrite(asyncResult); }
        public override int ReadByte() { DebugHelpers.WriteLine("ResponseFilter::ReadByte"); return m_outputStream.ReadByte(); }
        public override void WriteByte(byte value) { DebugHelpers.WriteLine("ResponseFilter::WriteByte"); m_outputStream.WriteByte(value); }

        protected override void Dispose(bool disposing) { DebugHelpers.WriteLine("ResponseFilter::Dispose"); base.Dispose(disposing); }
        protected override void ObjectInvariant() { DebugHelpers.WriteLine("ResponseFilter::ObjectInvariant"); base.ObjectInvariant(); }

    #endregion

        /// <summary>
        /// Helper for post-processing the response entity in order to replace any
        /// msgid nuggets such as [[[Translate me!]]] with the GetText string.
        /// </summary>
        /// <param name="entity">Subject HTTP response entity to be processed.</param>
        /// <param name="httpContext">
        /// Represents the current request.
        /// May be null when testing this interface. See remarks.
        /// </param>
        /// <returns>
        /// Processed (and possibly modified) entity.
        /// </returns>
        /// <remarks>
        /// An example replacement is as follows:
        /// <para>
        /// [[[Translate me!]]] -> Übersetzen mich!
        /// </para>
        /// This method supports a testing mode which is enabled by passing httpContext as null.
        /// In this mode, we output "test.message" for every msgid nugget.
        /// </remarks>
        public static string ProcessNuggets(string entity, HttpContextBase httpContext)
        {
            // Lookup any/all msgid nuggets in the entity and replace with any translated message.
            entity = m_regex_nugget.Replace(entity, delegate(Match match)
	            {
	                string msgid = match.Groups[1].Value;
                    string message = httpContext != null ? httpContext.GetText(msgid) : "test.message";
                    return message;
	            });

            return entity;
        }
        /// <summary>
        /// Helper for post-processing the response entity to append the passed string to
        /// URLs in the src attribute of script tags.
        /// </summary>
        /// <param name="entity">Subject HTTP response entity to be processed.</param>
        /// <param name="langtag">
        /// Langtag to be patched into URLs.
        /// </param>
        /// <returns>
        /// Processed (and possibly modified) entity.
        /// </returns>
        public static string PatchScriptUrls(string entity, string langtag)
        {
            return m_regex_script.Replace(
                entity,
                delegate(Match match)
                {
                    try {
                        string url = match.Groups[2].Value;
                        
                        // If URL is already localized...leave matched token alone.
                        string urlNonlocalized;
                        if (LocalizedApplication.UrlLocalizer.ExtractLangTagFromUrl(url, out urlNonlocalized) != null) {
                            return match.Groups[0].Value; } // original

                        // Localized the URL.
                        url = LocalizedApplication.UrlLocalizer.SetLangTagInUrlPath(url, langtag);

                        // Rebuild and return matched token.
                        string res = string.Format("{0}{1}{2}", 
                            match.Groups[1].Value,
                            url, 
                            match.Groups[3].Value);
                        return res;
                    }
                    catch (System.UriFormatException) {
                        return match.Groups[0].Value; // original
                    }
                });
        }
    }
}
