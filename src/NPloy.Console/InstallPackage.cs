using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using ManyConsole;

namespace NPloy
{
    public class InstallPackageCommand : ConsoleCommand
    {
        public InstallPackageCommand()
        {
            IsCommand("InstallPackage", "InstallRole");
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
                    Arguments =@"App_install\Install.ps1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            pProcess.StartInfo.WorkingDirectory = Package.Replace(' ', '.');

            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            Console.Write(strOutput);
            pProcess.WaitForExit();
            
            return 0;
        }
    }
}