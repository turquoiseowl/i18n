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
        /// <summary>
        /// Regular expression that controls the ContextTypes elligible for localization
        /// by the filter.
        /// </summary>
        /// <remarks>
        /// Defaults to text/html and application/javascript.
        /// Client may modify, for instance in Application_Start.
        /// </remarks>
        public static Regex m_regex_contenttypes = new Regex("^(?:text/html|application/javascript)$");

        private void HandleReleaseRequestState(object sender, EventArgs e)
        {
        // We get here once per request late on in the pipeline but prior to the response being filtered.
        // We take the opportunity to insert our filter in order to intercept that.
        //
            DebugHelpers.WriteLine("ResponseFilterModule::HandleReleaseRequestState -- sender: {0}, e:{1}", sender, e);
            
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("ResponseFilterModule::HandleReleaseRequestState -- ContentType: {0}", context.Response.ContentType);

            if (m_regex_contenttypes.Match(context.Response.ContentType).Success) {
                DebugHelpers.WriteLine("ResponseFilterModule::HandleReleaseRequestState -- Installing filter");
                context.Response.Filter = new ResponseFilter(context, context.Response.Filter);
            }
        }

    #region [IHttpModule]

        public void Init(HttpApplication application)
        {
            DebugHelpers.WriteLine("ResponseFilterModule::Init -- application: {0}", application);
            application.ReleaseRequestState += HandleReleaseRequestState;
        }

        public void Dispose() {}

    #endregion

    }

    public class ResponseFilter : Stream
    {
        /// <summary>
        /// Regex for finding and replacing msgid nuggets.
        /// </summary>
        protected static readonly Regex m_regex = new Regex(
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
            entity = ProcessEntity(entity, m_httpContext);

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
        /// Helper for post-processing the response entity.
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
        /// This method supports a testing mode which is enabled by passing httpContext as null.
        /// In this mode, we output "test.message" for every msgid nugget.
        /// </remarks>
        public static string ProcessEntity(string entity, HttpContextBase httpContext)
        {
            // Lookup any/all msgid nuggets in the entity and replace with any 
            entity = m_regex.Replace(entity, delegate(Match match)
	            {
	                string msgid = match.Groups[1].Value;
                    string message = httpContext != null ? httpContext.GetText(msgid) : "test.message";
                    return message;
	            });

            return entity;
        }
    }
}
