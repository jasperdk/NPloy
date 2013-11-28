using System;
using System.Collections.Generic;
using System.IO;
using ManyConsole;

namespace NPloy.Commands
{
    public class UninstallNodeCommand : ConsoleCommand
    {
        public UninstallNodeCommand()
        {
            IsCommand("UninstallNode", "UninstallNode");
            //HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Uninstall from this directory", s => WorkingDirectory = s);
        }

        //public string Node;
        public string WorkingDirectory;

        public override int Run(string[] remainingArguments)
        {
            if (!File.Exists(WorkingDirectory+@"\packages.config"))
            {
                Console.WriteLine("Nothing to uninstall");
                return 0;
            }
            var installedPackages = File.ReadAllLines(WorkingDirectory + @"\packages.config");

            foreach (var package in installedPackages)
            {
                var stopPackageCommand = new StopPackageCommand
                    {
                        Package = package,
                        WorkingDirectory = WorkingDirectory
                    };
                stopPackageCommand.Run(new string[0]);
            }

            foreach (var package in installedPackages)
            {
                var uninstallPackageCommand = new UninstallPackageCommand
                    {
                        Package = package,
                           WorkingDirectory = WorkingDirectory
                    };
                uninstallPackageCommand.Run(new string[0]);
            }

            File.Delete("packages.config");

            return 0;
        }
    }
}