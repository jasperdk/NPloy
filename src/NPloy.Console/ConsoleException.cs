using System;

namespace NPloy
{
    public class ConsoleException : Exception
    {
        public int ExitCode { get; set; }

        public ConsoleException(int exitCode)
        {
            ExitCode = exitCode;
        }
    }
}