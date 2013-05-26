using System.Text;

namespace i18n.MSBuild.Task
{
    class GetTextProcessException : GetTextException
    {
        public int ExitCode { get; set; }

        public GetTextProcessException(string process, int exitCode, string message) : base(process, message)
        {
            ExitCode = exitCode;
        }

        public GetTextProcessException(string process, int exitCode, StringBuilder message)
            : base(process, message)
        {
            ExitCode = exitCode;
        }

        public override string ToString()
        {
            return string.Format("Error while executing process {0}. " +
                                 "Process exited with code {1}. " +
                                 "Message was {2}", Process, ExitCode, Message);
        }

    }
}
