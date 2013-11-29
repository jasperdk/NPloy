using System;
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
        }

        //public string Node;
        public string WorkingDirectory;

        public override int Run(string[] remainingArguments)
        {
            if (!_nPloyConfiguration.HasInstalledPackages(WorkingDirectory))
            {
                Console.WriteLine("Nothing to uninstall");
                return 0;
            }
            var installedPackages = _nPloyConfiguration.GetInstalledPackges(WorkingDirectory);

            var stopNodeCommand = new StopNodeCommand
                {
                    WorkingDirectory = WorkingDirectory
                };
            stopNodeCommand.Run(new string[0]);

            foreach (var package in installedPackages)
            {
                var uninstallPackageCommand = new UninstallPackageCommand
                    {
                        WorkingDirectory = WorkingDirectory
                    };
                uninstallPackageCommand.Run(new[] { package });
            }

            _nPloyConfiguration.PackagesHasBeenUninstalled(WorkingDirectory);

            return 0;
        }
    }
}