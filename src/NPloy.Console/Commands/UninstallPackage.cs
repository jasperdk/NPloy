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
            HasOption("d|directory=", "Uninstall from this directory", s => WorkingDirectory = s);
        }

        public string Package;

        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {

            Package = Package.Replace(' ', '.');
            var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"powershell",
                    Arguments =@"App_install\Uninstall.ps1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = WorkingDirectory+@"\"+Package
                }
            };

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