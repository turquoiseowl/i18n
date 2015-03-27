using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.Helpers
{
    public static class MiscHelpers
    {
        /// <summary>
        /// Returns indication of whether the byte array passed contains UTF-8 text
        /// with a UTF-8 BOM prefix.
        /// </summary>
        public static bool IsTextWithBom_Utf8(this Byte[] buf)
        {
            return buf != null
                && buf.Length >= 3
                && buf[0] == 0xEF
                && buf[1] == 0xBB
                && buf[2] == 0xBF;
        }
    }
}
