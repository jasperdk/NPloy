using System;

namespace NPloy
{
    [Serializable]
    public sealed class ConsoleException : Exception
    {
        public int ExitCode { get; set; }

        public ConsoleException(int exitCode)
        {
            ExitCode = exitCode;
        }
    }
}