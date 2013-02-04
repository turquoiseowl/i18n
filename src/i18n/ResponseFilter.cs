using System;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace i18n
{
    /// <summary>
    /// A filter class used to intercept the ASP.NET response stream and
    /// post-process the response for localization.
    /// </summary>
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

        /// <summary>
        /// Regex for finding and replacing urls in html.
        /// </summary>
        protected static readonly Regex m_regex_html_urls = new Regex(
            "(?<pre><(?:script|img|a|area|link|base|input|frame|iframe|form)\\b.*?(?:src|href|action)\\s*=\\s*[\"']\\s*)(?<url>.+?)(?<post>\\s*[\"'][^>]*?>)",
            //"(?<pre><[.\\s]*?(?:src|href|action)\\s*=\\s*[\"']\\s*)(?<url>.+?)(?<post>\\s*[\"'][^>]*?>)",
            //"(?<pre><.*(?:src|href|action)\\s*=\\s*[\"']\\s*)(?<url>.+?)(?<post>\\s*[\"'][^>]*?>)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                // The above supports most common ways for a URI to appear in HTML/XHTML.
                // Note that if we fail to catch a URL here, it is not fata; only means we don't avoid a redirect
                // round-trip in some cases.
                // TODO: scope for improvement here:
                //  1. Restrict pairing between element and attribute e.g. "script" goes with "src" but not "href".
                //     This will probably require multiple passes with separate regex, one for each attribute name.
                // \s = whitespace
                // See also: http://www.w3.org/TR/REC-html40/index/attributes.html
                // See also: http://www.mikesdotnetting.com/Article/46/CSharp-Regular-Expressions-Cheat-Sheet

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
            entity = ProcessNuggets(m_httpContext, entity);

            // Patch all URLs in the entity which are:
            // 1. local (non-remote)
            // 2. are not already localized
            // This will save a redirect on subsequent requests to those URLs by the user-agent.
            // Examples of attributes containing urls include:
            //   <script src="..."> tags
            //   <img src="..."> tags
            //   <a href="..."> tags
            //   <link href="..."> tags
            //#37 TODO: Test embedded tags e.g. <a src="..."><img  src="..."/><a> etc.
            entity = PatchHtmlUrls(
                m_httpContext.Request.Url,
                entity, 
                m_httpContext.GetPrincipalAppLanguageForRequest().ToString(),
                LocalizedApplication.UrlLocalizer);

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
        /// <param name="entity">
        /// Subject HTTP response entity to be processed.
        /// </param>
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
        public static string ProcessNuggets(HttpContextBase httpContext, string entity)
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
        /// URLs in the src/href/action attribute of tags in the passed html.
        /// </summary>
        /// <param name="requestUrl">
        /// The URL of the current request, or null if the call is not associated with
        /// a specific URL. If set, only URLs relatively local to the requested URL are subject to being patched.
        /// </param>
        /// <param name="entity">
        /// Subject HTTP response entity to be processed.
        /// </param>
        /// <param name="langtag">
        /// Langtag to be patched into URLs.
        /// </param>
        /// <returns>
        /// Processed (and possibly modified) entity.
        /// </returns>
        public static string PatchHtmlUrls(
            Uri requestUrl,
            string entity, 
            string langtag, 
            IUrlLocalizer urlLocalizer)
        {
            return m_regex_html_urls.Replace(
                entity,
                delegate(Match match)
                {
                    try {
                        string url = match.Groups[2].Value;
                        
                        // If URL is already localized...leave matched token alone.
                        string urlNonlocalized;
                        if (urlLocalizer.ExtractLangTagFromUrl(url, UriKind.RelativeOrAbsolute, out urlNonlocalized) != null) {
                            return match.Groups[0].Value; } // original

                        // If URL is not local (i.e. remote host)...leave matched token alone.
                        if (requestUrl != null && !requestUrl.IsLocal(url)) {
                            return match.Groups[0].Value; } // original

                        // Is URL explicitly excluded from localization?
                        if (!LocalizedApplication.UrlLocalizer.FilterOutgoing(url, requestUrl)) {
                            return match.Groups[0].Value; } // original

                        // Localized the URL.
                        url = urlLocalizer.SetLangTagInUrlPath(url, UriKind.RelativeOrAbsolute, langtag);

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
