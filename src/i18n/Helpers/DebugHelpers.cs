using System;
using System.Diagnostics;
using System.Threading;

namespace i18n.Helpers
{
    internal class DebugHelpers
    {
        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            string str = String.Format("+++> {0}, Thread ID: {1} -- {2}",
                DateTime.Now.ToString(),
                Thread.CurrentThread.ManagedThreadId,
                message);
            Debug.WriteLine(str);
        }
        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            string str = String.Format("+++> {0}, Thread ID: {1} -- {2}",
                DateTime.Now.ToString(),
                Thread.CurrentThread.ManagedThreadId,
                format);
            Debug.WriteLine(string.Format(str, args));
        }
    }
}
