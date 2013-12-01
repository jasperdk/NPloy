using System;

namespace NPloy.Support
{
    public interface ICommandRunner
    {
        string RunCommand(string fileName, string arguments, string workingDirectory);
    }

    public class CommandRunner : ICommandRunner
    {
        public string RunCommand(string fileName, string arguments, string workingDirectory)
        {
            var pProcess = new System.Diagnostics.Process
                {
                    StartInfo =
                        {
                            FileName = fileName,
                            Arguments = arguments,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            WorkingDirectory = workingDirectory
                        }
                };

            pProcess.Start();
            pProcess.WaitForExit();
            if (pProcess.ExitCode > 0)
                throw new ConsoleException(pProcess.ExitCode);
            var strOutput = pProcess.StandardOutput.ReadToEnd();
            Console.Write(strOutput);
            return strOutput;

        }
    }
}