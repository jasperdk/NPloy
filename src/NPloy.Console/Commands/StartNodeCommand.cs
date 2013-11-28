using System;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class StartNodeCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;

        public StartNodeCommand()
            : this(new NPloyConfiguration())
        {

        }

        public StartNodeCommand(INPloyConfiguration nPloyConfiguration)
        {
            _nPloyConfiguration = nPloyConfiguration;
            IsCommand("StartNode", "StartNode");
            //HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Start packages installed in this directory", s => WorkingDirectory = s);
        }

        //public string Node;
        public string WorkingDirectory;

        public override int Run(string[] remainingArguments)
        {
            if (!_nPloyConfiguration.HasInstalledPackages(WorkingDirectory))
            {
                Console.WriteLine("Nothing to stop");
                return 0;
            }
            var installedPackages = _nPloyConfiguration.GetInstalledPackges(WorkingDirectory);

            foreach (var package in installedPackages)
            {
                var startPackageCommand = new StartPackageCommand
                    {
                        Package = package,
                        WorkingDirectory = WorkingDirectory
                    };
                var result = startPackageCommand.Run(new string[0]);
                if (result > 0)
                    return result;
            }

            return 0;
        }
    }
}