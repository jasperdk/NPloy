using System;
using System.IO;
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
            try
            {
                SetDefaultOptionValues();

                if (!_nPloyConfiguration.HasInstalledPackages(WorkingDirectory))
                {
                    Console.WriteLine("Nothing to start");
                    return 0;
                }
                Console.WriteLine("Start node in: " + WorkingDirectory);
                var installedPackages = _nPloyConfiguration.GetInstalledPackges(WorkingDirectory);

                foreach (var package in installedPackages)
                {
                    var startPackageCommand = new StartPackageCommand
                        {
                            WorkingDirectory = WorkingDirectory
                        };
                    var result = startPackageCommand.Run(new[] { package });
                    if (result > 0)
                        return result;
                }

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void SetDefaultOptionValues()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = currentDirectory;
            if (!Path.IsPathRooted(WorkingDirectory))
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), WorkingDirectory);
        }
    }
}