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
            //HasOption("d|directory=", "Install to this directory", s => WorkingDirectory = s);
        }

        //public string Node;
        //public string WorkingDirectory;

        public override int Run(string[] remainingArguments)
        {
            if (!File.Exists("packages.config"))
            {
                Console.WriteLine("Nothing to uninstall");
                return 0;
            }
            var installedPackages = File.ReadAllLines("packages.config");

            foreach (var package in installedPackages)
            {
                var stopPackageCommand = new StopPackageCommand {  };
                stopPackageCommand.Run(new[]{package});
            }

            foreach (var package in installedPackages)
            {
                var uninstallPackageCommand = new UninstallPackageCommand { };
                uninstallPackageCommand.Run(new[] { package });
            }

            File.Delete("packages.config");

            return 0;
        }
    }
}