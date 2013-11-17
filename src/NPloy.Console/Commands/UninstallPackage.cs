using System;
using ManyConsole;

namespace NPloy.Commands
{
    public class UninstallPackageCommand : ConsoleCommand
    {
        public UninstallPackageCommand()
        {
            IsCommand("UninstallPackage", "UninstallPackage");
            HasAdditionalArguments(1, "Package");
        }

        public string Package;

        public override int Run(string[] remainingArguments)
        {

            Package = remainingArguments[0];
            var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"powershell",
                    Arguments =@"App_install\Uninstall.ps1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            pProcess.StartInfo.WorkingDirectory = Package.Replace(' ', '.');

            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            Console.Write(strOutput);
            pProcess.WaitForExit();
            if (pProcess.ExitCode != 0)
                throw new ConsoleException(pProcess.ExitCode);
            return 0;
        }
    }
}