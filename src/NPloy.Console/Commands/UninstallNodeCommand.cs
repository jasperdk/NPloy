using System;
using System.Collections.Generic;
using System.IO;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class UninstallNodeCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;

        public UninstallNodeCommand()
            : this(new NPloyConfiguration())
        {

        }

        public UninstallNodeCommand(INPloyConfiguration nPloyConfiguration)
        {
            _nPloyConfiguration = nPloyConfiguration;
            IsCommand("UninstallNode", "UninstallNode");
            //HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Uninstall from this directory", s => WorkingDirectory = s);
            HasOption("r|remove", "Delete files and directories after uninstall", s => RemoveFilesAndDirectories = s != null);
        }

        //public string Node;
        public string WorkingDirectory;
        public bool RemoveFilesAndDirectories;

        public override int Run(string[] remainingArguments)
        {
            try
            {
                SetDefaultOptionValues();

                if (!_nPloyConfiguration.HasInstalledPackages(WorkingDirectory))
                {
                    Console.WriteLine("Nothing to uninstall");
                    return 0;
                }

                Console.WriteLine("Uninstall node in: " + WorkingDirectory);
                
                var installedPackages = _nPloyConfiguration.GetInstalledPackges(WorkingDirectory);
                StopNode();
                UninstallPackages(installedPackages);
                if (RemoveFilesAndDirectories)
                {
                    DeleteDirectories(installedPackages);
                }

                _nPloyConfiguration.PackagesHasBeenUninstalled(WorkingDirectory);

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private static void DeleteDirectories(IEnumerable<string> installedPackages)
        {
            foreach (var installedPackage in installedPackages)
            {
                Directory.Delete(installedPackage, true);
            }
        }

        private void UninstallPackages(IEnumerable<string> installedPackages)
        {
            foreach (var package in installedPackages)
            {
                var uninstallPackageCommand = new UninstallPackageCommand
                    {
                        WorkingDirectory = WorkingDirectory
                    };
                uninstallPackageCommand.Run(new[] {package});
            }
        }

        private void StopNode()
        {
            var stopNodeCommand = new StopNodeCommand
                {
                    WorkingDirectory = WorkingDirectory
                };
            stopNodeCommand.Run(new string[0]);
        }

        private void SetDefaultOptionValues()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = currentDirectory;
        }
    }
}