using System;
using System.Diagnostics;
using System.Text;

namespace i18n.Helpers
{
    internal class DebugHelpers
    {
        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            string str = String.Format("+++> {0} -- {1}",
                DateTime.Now.ToString(),
                message);
            Debug.WriteLine(str);
        }
        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            string str = String.Format("+++> {0} -- {1}",
                DateTime.Now.ToString(),
                format);
            Debug.WriteLine(str, args);
        }
    }
}
