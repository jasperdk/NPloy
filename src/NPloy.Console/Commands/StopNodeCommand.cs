using System;
using System.IO;
using System.Linq;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class StopNodeCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;

        public StopNodeCommand()
            : this(new NPloyConfiguration())
        {

        }

        public StopNodeCommand(INPloyConfiguration nPloyConfiguration)
        {
            _nPloyConfiguration = nPloyConfiguration;
            IsCommand("StopNode", "StopNode");
            //HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Stop installed packages in this directory", s => WorkingDirectory =s);
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
                    Console.WriteLine("Nothing to stop");
                    return 0;
                }
                Console.WriteLine("Stop node in: " + WorkingDirectory);
                var installedPackages = _nPloyConfiguration.GetInstalledPackges(WorkingDirectory);

                foreach (var package in installedPackages.Reverse())
                {
                    var stopPackageCommand = new StopPackageCommand
                        {
                            WorkingDirectory = WorkingDirectory
                        };
                    var result = stopPackageCommand.Run(new[] { package.FullName });
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