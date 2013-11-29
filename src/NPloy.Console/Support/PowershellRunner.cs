using System;

namespace NPloy.Support
{
    public interface IPowershellRunner
    {
        string RunPowershellScript(string script, string workingDirectory);
    }

    public class PowerShellRunner : IPowershellRunner
    {
        public string RunPowershellScript(string script, string workingDirectory)
        {
            var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"powershell",
                    Arguments = "-executionpolicy unrestricted " + script,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = workingDirectory
                }
            };

            pProcess.Start();
            var strOutput = pProcess.StandardOutput.ReadToEnd();
            Console.Write(strOutput);
            pProcess.WaitForExit();
            if (pProcess.ExitCode != 0)
                throw new ConsoleException(pProcess.ExitCode);
            return strOutput;
        }
    }
}