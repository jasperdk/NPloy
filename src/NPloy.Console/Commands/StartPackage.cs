using System;
using System.IO;
using ManyConsole;

namespace NPloy.Commands
{
    public class StartPackageCommand : ConsoleCommand
    {
        public StartPackageCommand()
        {
            IsCommand("StartPackage", "StartPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("d|directory=", "Start from this directory", s => WorkingDirectory = s);
        }

        public string Package;

        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            Package = Package.Replace(' ', '.');

            if (!File.Exists(@"App_Install\Start.ps1"))
                return 1;

            Console.WriteLine("Running start scripts for package: " + Package);
            var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"powershell",
                    Arguments =@".\App_Install\Start.ps1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = WorkingDirectory+@"\"+Package
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