using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Web;
using i18n.Helpers;

namespace i18n
{
    /// <summary>
    /// A filter class used to intercept the ASP.NET response stream and
    /// post-process the response for localization. This includes:
    ///   1. Localization of marked messages (nuggets) in the response entity;
    ///   2. Late URL Localization.
    /// </summary>
    public class ResponseFilter : Stream
    {
        private IEarlyUrlLocalizer m_earlyUrlLocalizer;
        private INuggetLocalizer m_nuggetLocalizer;

        /// <remarks>
        /// We need to accumulate all written blocks into a staging buffer so that
        /// any nuggets which straddle the break between two blocks are picked up
        /// correctly. This approach is not perfect in that we need to allocate a block
        /// of memory for the entire response, which could be large, but the only way
        /// around would involve parsing for nuggest where we track start and end
        /// tokens (that is, don't use regex).
        /// </remarks>
        private MemoryStream m_stagingBuffer = new MemoryStream();

        /// <summary>
        /// The stream onto which we pass data once processed. This will typically be set 
        /// to the stream which was the original value of Response.Filter before we got there.
        /// </summary>
        protected Stream m_outputStream;

        /// <summary>
        /// HTTP context with which the filter is associated.
        /// </summary>
        protected HttpContextBase m_httpContext;

        public ResponseFilter(
            HttpContextBase httpContext, 
            Stream outputStream,
            IEarlyUrlLocalizer earlyUrlLocalizer,
            INuggetLocalizer nuggetLocalizer)
        {
            m_httpContext = httpContext;
            m_outputStream = outputStream;
            m_earlyUrlLocalizer = earlyUrlLocalizer;
            m_nuggetLocalizer = nuggetLocalizer;
        }

    #region [Stream]

        public override void Write(byte[] buffer, int offset, int count)
        {
            DebugHelpers.WriteLine("ResponseFilter::Write -- count: {0}", count);

            m_stagingBuffer.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            DebugHelpers.WriteLine("ResponseFilter::Flush");

            // Convert byte array into string.
            Encoding enc = m_httpContext.Response.ContentEncoding;
            Byte[] buf = m_stagingBuffer.GetBuffer();
            string entity = enc.GetString(buf, 0, (int)m_stagingBuffer.Length);

            // Prep for special BOM handling.
            // NB: at present we only support UTF-8 for this logic.
            //bool utf8WithoutBom = enc is UTF8Encoding && !buf.IsTextWithBom_Utf8();
                // #86 -- disabled this BOM handling for now as it doesn't seem to help.
                // Furthermore, it appears that the Encoding instance returned by 
                // Response.ContentEncoding above is correctly configured to omit
                // BOM or not from its GetByte method, depending on whether or not
                // the response buffer has a BOM in it or not (for instance, see the
                // ctor of UTF8Encoding that takes a bool for this).

            // Buffer no longer required so release memory.
            m_stagingBuffer.Dispose();
            m_stagingBuffer = null;
            buf = null;

            // Translate any embedded messages aka 'nuggets'.
            if (m_nuggetLocalizer != null)
            {
                entity = m_nuggetLocalizer.ProcessNuggets(
                    entity,
                    m_httpContext.GetRequestUserLanguages());
            }

            // If Early Localization is enabled, we balance that here with Late URL Localization.
            // The goal is to localize same-host URLs in the entity body and so save a redirect 
            // on subsequent requests to those URLs by the user-agent (Early URL Localization).
            // We patch all URLs in the entity which are:
            //  1. same-host
            //  2. are not already localized
            //  3. pass any custom filtering
            // Examples of attributes containing urls include:
            //   <script src="..."> tags
            //   <img src="..."> tags
            //   <a href="..."> tags
            //   <link href="..."> tags
            if (m_earlyUrlLocalizer != null)
            {
                entity = m_earlyUrlLocalizer.ProcessOutgoing(
                    entity, 
                    m_httpContext.GetPrincipalAppLanguageForRequest().ToString(),
                    m_httpContext);
            }

            //DebugHelpers.WriteLine("ResponseFilter::Write -- entity:\n{0}", entity);

            // Render the string back to an array of bytes.
            buf = enc.GetBytes(entity);
            enc = null; // release memory asap.
            int count = buf.Length;

            // Prep to skip any BOM if it wasn't originally there.
            // NB: at present we only support UTF-8 for this logic.
            int skip = 0;
            //if (utf8WithoutBom && buf.IsTextWithBom_Utf8()) {
            //    skip = 3; }
                // #86 -- see matching comment above.

            // Forward data on to the original response stream.
            m_outputStream.Write(buf, skip, count -skip);

            // Complete the write.
            m_outputStream.Flush();
        }

        // The following overrides may be unnecessary. Instead we could have derived this class
        // from MemoryStream or something like that which was the original approach.
        // However, some odd behaviour occurred when doing this and these methods were wired
        // in to diagnose. Problems have gone away now that we derived straight from Stream
        // and cause was not found.

        public override bool CanRead  { get { DebugHelpers.WriteLine("ResponseFilter::CanRead::get"); return m_outputStream.CanRead; } }
        public override bool CanSeek  { get { DebugHelpers.WriteLine("ResponseFilter::CanSeek::get"); return m_outputStream.CanSeek; } }
        public override bool CanWrite { get { DebugHelpers.WriteLine("ResponseFilter::CanWrite::get"); return m_outputStream.CanWrite; } }
        public override long Length   { get { DebugHelpers.WriteLine("ResponseFilter::Length::get"); return m_outputStream.Length; } }
        public override long Position { get { DebugHelpers.WriteLine("ResponseFilter::Position::get"); return m_outputStream.Position; } set { DebugHelpers.WriteLine("ResponseFilter::Position::set"); m_outputStream.Position = value; } }
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

    }
}
