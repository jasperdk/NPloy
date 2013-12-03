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
            Console.WriteLine("Executing command: " + fileName);
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
            Console.Write(strOutput);

            pProcess.WaitForExit();

            if (pProcess.ExitCode > 0)
                throw new ConsoleException(pProcess.ExitCode);

            return strOutput;

        }
    }
}