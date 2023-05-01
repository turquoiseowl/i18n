using System;
using System.IO;
using System.Text;
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
        private bool m_streamIsCompressed = false;

        /// <remarks>
        /// We need to accumulate all written blocks into a staging buffer so that
        /// any nuggets which straddle the break between two blocks are picked up
        /// correctly. This approach is not perfect in that we need to allocate a block
        /// of memory for the entire response, which could be large, but the only way
        /// around would involve parsing for nuggest where we track start and end
        /// tokens (that is, don't use regex).
        /// </remarks>
        //private MemoryStream m_stagingBuffer = new MemoryStream();
        private StringBuilder sbNugget;
        /* Translation state machine */
        private int numBlock = 0;
        private int numCharsStartToken = 0;
        private int numCharsEndToken = 0;
        /* Async postback sections state machine */
        private string[] typesToTranslate = null;
        private int sectionLen;
        private StringBuilder sbSection = new StringBuilder();
        private string sectionType, sectionId;
        private enum AsyncPostbackSectionState { ReadingLen, ReadingType, ReadingId, ReadingContent };
        private AsyncPostbackSectionState asyncPostbackSectionState = AsyncPostbackSectionState.ReadingLen;
        private int sectionContentProcessed = 0;
        private bool isSectionTypeTranslated = false;

        /// <summary>
        /// The stream onto which we pass data once processed. This will typically be set 
        /// to the stream which was the original value of Response.Filter before we got there.
        /// </summary>
        protected Stream m_outputStream;
#if DEBUG
        private MemoryStream debug_outputStream = new MemoryStream();
        private int totalTranslated = 0, totalUntranslated = 0, total = 0;
#endif
        /// <summary>
        /// HTTP context with which the filter is associated.
        /// </summary>
        protected System.Web.HttpContextBase m_httpContext;

        public ResponseFilter(
            System.Web.HttpContextBase httpContext, 
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
        private char[] startTokenSymbol = new char[] { '[', '[', '[' };
        /// <summary>
        /// JRL,2019
        /// Translate all nuggets in a string. It does not code the characters.
        /// </summary>
        /// <param name="stream">Stream to write (already coded to the active encoding of the response</param>
        /// <param name="entity"></param>
        /// <param name="arrEntity">To save conversions to chararray</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>Number of no-encoded characters for the translated string. Writed characters number may be greater than original stream! (due to encoding)</returns>
        private int Translate(Stream stream, string entity, char[] arrEntity, int offset, int count)
        {
            int charsWritten = 0; // number of chars written to the stream (no number of bytes)

            Encoding enc = m_httpContext.Response.ContentEncoding;

            int lastUnwrittenCharIndex = offset;
            int entityLength = Math.Min(entity.Length, offset + count);

            for (int i = offset; i < entityLength; i++)
            {
                //no token, look for start sequence
                if (sbNugget == null)
                {
                    if (entity[i] != '[')
                    {
                        // Write previous [ chars
                        if (i == offset && numCharsStartToken > 0)
                        {
                            byte[] buf = enc.GetBytes(startTokenSymbol, 0, numCharsStartToken);
                            stream.Write(buf, 0, buf.Length);
                            charsWritten += numCharsStartToken;   //chars! (no bytes)
                            //bytesWritten += buf.Length;
                        }
                        numCharsStartToken = 0;
                        continue;
                    }  // '[': startnuggettoken[numCharsEndToken]
                    numCharsStartToken++;
                    if (numCharsStartToken == 3)  // 3: startnuggettoken.length
                    {
                        // write previous inter-nugget characters
                        if (i - 3 + 1 - lastUnwrittenCharIndex > 0)
                        {
                            byte[] buf = enc.GetBytes(arrEntity, lastUnwrittenCharIndex, i - 3 + 1 - lastUnwrittenCharIndex);
                            stream.Write(buf, 0, buf.Length);
                            //bytesWritten += buf.Length;
                            charsWritten += (i - 3 + 1 - lastUnwrittenCharIndex); //chars, no bytes!
                        }

                        sbNugget = new StringBuilder();
                        sbNugget.Append("[[[");  // starttoken
                        numCharsStartToken = 0;
                    }

                }
                else // reading a token: look for end sequence
                {
                    sbNugget.Append(entity[i]);
                    if (entity[i] != ']') { numCharsEndToken = 0; continue; }
                    numCharsEndToken++;
                    if (numCharsEndToken == 3)  // 3: starttoken.length
                    { // token: end
                        numCharsEndToken = 0;

                        // Translate any embedded messages aka 'nuggets'.
                        if (m_nuggetLocalizer != null)
                        {
                            string strNugget = m_nuggetLocalizer.ProcessNuggets(
                                sbNugget.ToString(),
                                m_httpContext.GetRequestUserLanguages());
                            //DebugHelpers.WriteLine("ResponseFilter::Translate -- nugget {0} -> {1}", sbNugget.ToString(), strNugget);

                            // Render the string back to an array of bytes.
                            byte[] buf = enc.GetBytes(strNugget);
                            stream.Write(buf, 0, buf.Length);
                            charsWritten += strNugget.Length;  //char length value before encoding: chars, no bytes!
                            //bytesWritten += buf.Length;
                        }
                        lastUnwrittenCharIndex = i + 1;
                        // no nugget
                        sbNugget = null;
                    }
                }
            }
            // Write the remaining chars
            if (sbNugget == null)
            {
                // Render the string back to an array of bytes, only if it's required
                if (entityLength - lastUnwrittenCharIndex - numCharsStartToken > 0)
                {
                    byte[] buf = enc.GetBytes(arrEntity, lastUnwrittenCharIndex, entityLength - lastUnwrittenCharIndex - numCharsStartToken);
                    stream.Write(buf, 0, buf.Length);
                    //bytesWritten += buf.Length;
                    charsWritten += (entityLength - lastUnwrittenCharIndex - numCharsStartToken);
                }
            }
            return charsWritten;
        }

        private MemoryStream memoryStream = new MemoryStream();
        bool isProcessedHeaders = false;
        //        byte[] bufRestoChunk = null; //resto de chunk anterior no escrito, si hay

        public override void Write(byte[] buffer, int offset, int count)
        {
            //If this is the first Write for a compressed stream and it includes the gzip magic number in the first two bytes (hex 1F 8B, dec 31 139)
            //then set the filter flag to indicate that the stream is compressed and pass through this Write
            //If we set the flag here then the Flush will also pass through later
            //Note that we also have a check in LocalizingModule for the response Content-Encoding header being set to "gzip", which should prevent
            //the filter from being installed, but this checks the actual content in the stream in case we get here for a compressed stream
            if (m_streamIsCompressed || (numBlock == 0 && buffer.Length >= 2 && buffer[0] == 31 && buffer[1] == 139))
            {
                DebugHelpers.WriteLine("ResponseFilter::Write -- skipping compressed content");
                m_streamIsCompressed = true;
            }
            Encoding enc = m_httpContext.Response.ContentEncoding;
            string entity = enc.GetString(buffer, offset, count);
            char[] arrEntity;

            DebugHelpers.WriteLine("ResponseFilter::Write -- count: {0}, decodedcount: {2}, entity: {1, 20}...", count, entity, entity.Length);

            var page = m_httpContext.Handler as System.Web.UI.Page;
            bool isScriptManager = false;
            if (page != null)
            {
                var sm = System.Web.UI.ScriptManager.GetCurrent(page);
                if (sm != null && sm.IsInAsyncPostBack) isScriptManager = true;
                if (page.IsPostBack && isScriptManager)
                { //#178
                    if (typesToTranslate == null)
                        typesToTranslate = LocalizedApplication.Current.AsyncPostbackTypesToTranslate.Split(new char[] { ',' });
                }
            }

            if (!isProcessedHeaders && m_earlyUrlLocalizer != null)
            {
                m_earlyUrlLocalizer.ProcessOutgoingHeaders(
                    m_httpContext.GetPrincipalAppLanguageForRequest().ToString(),
                    m_httpContext);
                isProcessedHeaders = true;
            }

            // Not async postback
            if (!isScriptManager)
            {
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
                    entity = m_earlyUrlLocalizer.ProcessOutgoingNuggets(
                        entity,
                        m_httpContext.GetPrincipalAppLanguageForRequest().ToString(),
                        m_httpContext);
                }
                arrEntity = entity.ToCharArray(); // to avoid converting to array more than once
                Translate(m_outputStream, entity, arrEntity, 0, entity.Length);
            }
            // async postback
            else
            {
                int unprocessedSectionLenIndex = 0;
                int unprocessedSectionContentIndex = 0;
                int entityLength = entity.Length;
                arrEntity = entity.ToCharArray(); // to avoid converting to array more than once

                for (int i = 0; i < entityLength; i++)
                {
                    switch (asyncPostbackSectionState)
                    {
                        case AsyncPostbackSectionState.ReadingLen:
                            if (entity[i] != '|')
                            {
                                sbSection.Append(entity[i]);
                                continue;
                            }
                            sectionLen = Convert.ToInt32(sbSection.ToString());
                            sbSection.Clear();
                            asyncPostbackSectionState = AsyncPostbackSectionState.ReadingType;
                            break;
                        case AsyncPostbackSectionState.ReadingType:
                            if (entity[i] != '|')
                            {
                                sbSection.Append(entity[i]);
                                continue;
                            }
                            sectionType = sbSection.ToString();
                            sbSection.Clear();
                            // Check if this type needs to be translated
                            isSectionTypeTranslated = false;
                            foreach (string type in typesToTranslate)
                                if (sectionType.Equals(type))
                                {
                                    isSectionTypeTranslated = true;
                                    break;
                                }
                            //if (isSectionTypeTranslated) // buffer for content
                            //    memoryStream = new MemoryStream(sectionLen);
                            asyncPostbackSectionState = AsyncPostbackSectionState.ReadingId;
                            break;
                        case AsyncPostbackSectionState.ReadingId:
                            if (entity[i] != '|')
                            {
                                sbSection.Append(entity[i]);
                                continue;
                            }
                            sectionId = sbSection.ToString();
                            sbSection.Clear();
                            asyncPostbackSectionState = AsyncPostbackSectionState.ReadingContent;
                            unprocessedSectionContentIndex = i + 1;  // next one
                            break;
                        case AsyncPostbackSectionState.ReadingContent:
                            if (sectionContentProcessed < sectionLen)
                            {
                                sectionContentProcessed++;
                                sbSection.Append(entity[i]);
                                continue;
                            }
                            // (At this i index a '|' char must exist).

                            string sectionContent = sbSection.ToString();
                            sbSection.Clear();

                            // this  '|' is not at the last or first position of entity
                            // translate (without '|' separator)
                            int bytesWritten = 0; //javascript usa substr y charAt, or lo que 2octetos son un char para él en caracteres especiales
                            byte[] buf = null;
                            if (sectionContent.Length > 0)
                            {
                                // Traduce
                                if (isSectionTypeTranslated)
                                {
                                    bytesWritten = Translate(memoryStream, sectionContent, sectionContent.ToCharArray(), 0, sectionContent.Length);
                                    DebugHelpers.WriteLine("ResponseFilter::Translate -- sectionContent: L={0}, L_encoded={2}, V={1,10}...", sectionContent.Length, sectionContent, memoryStream.Length);
                                }
                                else // No traducido
                                {
                                    buf = enc.GetBytes(sectionContent);
                                    //bytesWritten = buf.Length; //bytes!
                                    bytesWritten = sectionContent.Length; //chars!
                                    DebugHelpers.WriteLine("ResponseFilter::No translate -- sectionContent: L={0}, L_encoded={2}, V={1,10}...", sectionContent.Length, sectionContent, buf.Length);
                                }
                            }

                            // write section: len|type|id 
                            sbSection.Append(bytesWritten);  //content length (bytes encoded)
                            sbSection.Append('|');
                            sbSection.Append(sectionType);
                            sbSection.Append('|');
                            sbSection.Append(sectionId);
                            sbSection.Append('|');
                            // Escribe prefijo
                            byte[] bufPrefijo = enc.GetBytes(sbSection.ToString());
                            sbSection.Clear();
                            m_outputStream.Write(bufPrefijo, 0, bufPrefijo.Length);  //bytes y chars coinciden porque no hay especiales
#if DEBUG
                            debug_outputStream.Write(bufPrefijo, 0, bufPrefijo.Length);
#endif
                            // Escribe contenido
                            if (sectionContent.Length > 0)
                            {
                                if (isSectionTypeTranslated)
                                {
                                    DebugHelpers.WriteLine("ResponseFilter::Write -- Translated bytesWritten={0} encoded(memoryStream.Length)={1}", bytesWritten, memoryStream.Length);
#if DEBUG
                                    long before = memoryStream.Position;
                                    memoryStream.Position = 0; //copia desde posición 0 a length
                                    memoryStream.CopyTo(debug_outputStream);
                                    memoryStream.Position = before;
#endif
                                    // Escribe
                                    memoryStream.WriteTo(m_outputStream);
                                    // Reutiliza memoryStream
                                    memoryStream.Position = 0; //reutiliza
                                    memoryStream.SetLength(0);
                                    // memoryStream.Close();
                                    // memoryStream.Dispose();
                                    // memoryStream = null;
                                }
                                else  // No traducido
                                {
                                    DebugHelpers.WriteLine("ResponseFilter::Write -- NO Translated charsWritten={0} encoded(buf.Length)={1}", bytesWritten, buf.Length);
                                    m_outputStream.Write(buf, 0, buf.Length);
#if DEBUG
                                    debug_outputStream.Write(buf, 0, buf.Length);
#endif
                                }
                            }
                            // '|' separator
                            buf = enc.GetBytes("|"); // inserta separator del item anterior
                            m_outputStream.Write(buf, 0, buf.Length); //byte/char coinciden no es especial
#if DEBUG
                            debug_outputStream.Write(buf, 0, buf.Length);
#endif
                            asyncPostbackSectionState = AsyncPostbackSectionState.ReadingLen;
                            sectionContentProcessed = 0;
                            unprocessedSectionLenIndex = i + 1;
                            break;
                    }
                }
            }
        }

        public override void Flush()
        {
            DebugHelpers.WriteLine("ResponseFilter::Flush");

            m_outputStream.Flush();

#if DEBUG
            // Check
            Encoding enc = m_httpContext.Response.ContentEncoding;

            int numtoken = 1;
            //BinaryReader br = new BinaryReader(debug_outputStream);
            StreamReader sr = new StreamReader(debug_outputStream, enc);  // lee chars con la codificación dada

            sr.BaseStream.Position = 0;
            while (sr.BaseStream.Position != sr.BaseStream.Length)
            {
                bool isNuevo = (sr.BaseStream.Position == 0);

                // LEN|
                char[] arrlen = new char[7]; //ceros
                int i = 0;
                while ((i < 7) && (arrlen[i++] = (char)sr.Read()) != '|')
                    ; //recoge len
                if ((isNuevo) && (i != 2))
                    break; // termina bucle si no encuentra "1|" en las 2 primeras posiciones del fichero de salida

                // ES POST ASÍNCRONO

                arrlen[i - 1] = '\0'; // quita el |
                string slen = (i > 1 ? new string(arrlen) : "");

                int len = 0;

                if (!Int32.TryParse(slen, out len))
                {
                    DebugHelpers.WriteLine("ResponseFilter::Flush - Delta: ERROR numDelta: {0} slen: '{1}'", numtoken, slen);
                    break;
                }

                // ID|

                while (((char)sr.Read()) != '|')
                    ; //avanza id

                // TYPE|
                char[] type = new char[256];
                i = 0;
                while ((type[i++] = (char)sr.Read()) != '|')
                    ; //avanza type
                type[i - 1] = '\0';
                string stype = (i > 1) ? new string(type) : "";

                // CONTENT  : importante: lee len chars, no bytes (están codificados)
                for (i = 0; i < len; i++)
                    sr.Read();

                // Comprueba '|'
                char actual;
                if (sr.BaseStream.Position == sr.BaseStream.Length || ((actual = (char)sr.Read()) == '|'))
                {
                    DebugHelpers.WriteLine(String.Format("ResponseFilter::Flush - Delta OK: numDelta:{0} len:{1} type:'{2}' ok.", numtoken, len, stype.Substring(0, Math.Min(10, stype.Length))));
                }
                else // error
                {
                    DebugHelpers.WriteLine(String.Format("ResponseFilter::Flush - Delta ERROR!!: numDelta:{0} len:{1} ERROR !!!", numtoken, len));
                    break; //para bucle
                }
                numtoken++;
            }
#endif

        }


        // JRL,2019,
        /* public override void Write(byte[] buffer, int offset, int count)
        {
            DebugHelpers.WriteLine("ResponseFilter::Write -- count: {0}", count);

            //If this is the first Write for a compressed stream and it includes the gzip magic number in the first two bytes (hex 1F 8B, dec 31 139)
            //then set the filter flag to indicate that the stream is compressed and pass through this Write
            //If we set the flag here then the Flush will also pass through later
            //Note that we also have a check in LocalizingModule for the response Content-Encoding header being set to "gzip", which should prevent
            //the filter from being installed, but this checks the actual content in the stream in case we get here for a compressed stream
            if (m_streamIsCompressed || (m_stagingBuffer.Length == 0 && buffer.Length >= 2 && buffer[0] == 31 && buffer[1] == 139))
            {
                DebugHelpers.WriteLine("ResponseFilter::Write -- skipping compressed content");
                m_streamIsCompressed = true;
                m_outputStream.Write(buffer, offset, count);
                return;
            }
            else
            {
                m_stagingBuffer.Write(buffer, offset, count);
            }
        }

        public override void Flush()
        {
            if (m_stagingBuffer == null) { return; } 

            DebugHelpers.WriteLine("ResponseFilter::Flush");

            Byte[] buf = m_stagingBuffer.GetBuffer();

            //If the buffer holds compressed content then we allow the original output stream to be used because we don't try to modify compressed streams
            if (m_streamIsCompressed)
            {
                DebugHelpers.WriteLine("ResponseFilter::Flush -- skipping compressed content");
                m_outputStream.Flush();
                return;
            }

            // Convert byte array into string.
            Encoding enc = m_httpContext.Response.ContentEncoding;
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
                var page = m_httpContext.Handler as System.Web.UI.Page;
                bool isScriptManager = false;
                if (page != null)
                {
                    var sm = System.Web.UI.ScriptManager.GetCurrent(page);
                    if (sm != null && sm.IsInAsyncPostBack) isScriptManager = true;
                }
                //if async postback
                if (page != null && page.IsPostBack && isScriptManager && !String.IsNullOrEmpty(entity) && !String.IsNullOrEmpty(entity.Replace("\r","").Split('\n')[0])) { //#178
                    var asyncPostbackParser = new AsyncPostbackParser(entity);
                    var types = LocalizedApplication.Current.AsyncPostbackTypesToTranslate.Split(new char[] {','});
                    foreach (var type in types) {
                        asyncPostbackParser.GetSections(type).ForEach(section => {
                            section.Content = m_nuggetLocalizer.ProcessNuggets(
                                section.Content,
                                m_httpContext.GetRequestUserLanguages());
                        });
                    }
                    entity = asyncPostbackParser.ToString();
                } else {
                    entity = m_nuggetLocalizer.ProcessNuggets(
                        entity,
                        m_httpContext.GetRequestUserLanguages());
                }
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
                var langtag = m_httpContext.GetPrincipalAppLanguageForRequest().ToString();

                entity = m_earlyUrlLocalizer.ProcessOutgoingNuggets(
                    entity, langtag,
                    m_httpContext);

                 m_earlyUrlLocalizer.ProcessOutgoingHeaders(langtag, m_httpContext);
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
        */

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
