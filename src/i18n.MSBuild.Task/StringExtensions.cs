using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.MSBuild.Task
{
    internal static class  StringExtensions
    {
        /// <summary>
        /// Format a given with input parameters.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return String.Format(format, args);
        }
    }
}
