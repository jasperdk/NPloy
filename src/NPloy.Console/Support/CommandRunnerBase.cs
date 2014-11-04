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
            Console.WriteLine("Executing command: {0}" , fileName);
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

            var strOutput = pProcess.StandardOutput.ReadToEnd();

            pProcess.WaitForExit();

            Console.WriteLine(strOutput);

            if (pProcess.ExitCode > 0)
            {
                Console.WriteLine("Executing command {0} failed with exit code {1}", fileName, pProcess.ExitCode);
                throw new ConsoleException(pProcess.ExitCode);
            }

            return strOutput;
        }
    }
}