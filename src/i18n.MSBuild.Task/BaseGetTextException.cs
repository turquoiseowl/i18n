using System;
using System.Text;

namespace i18n.MSBuild.Task
{
    class GetTextException : Exception
    {
        public string Process { get; set; }

        public GetTextException(string process, string message) : base(message)
        {
            Process = process;
        }

        public GetTextException(string process, StringBuilder message)
            : base(message.ToString())
        {
            Process = process;
        }

    }
}