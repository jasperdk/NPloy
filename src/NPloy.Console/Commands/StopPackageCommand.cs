using System;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class StopPackageCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly IPowershellRunner _powershellRunner;

        public StopPackageCommand()
            : this(new NPloyConfiguration(), new PowerShellRunner())
        {

        }

        public StopPackageCommand(INPloyConfiguration nPloyConfiguration,
            IPowershellRunner powershellRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _powershellRunner = powershellRunner;
            IsCommand("StopPackage", "StopPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("d|directory=", "Stop in this directory", s => WorkingDirectory = s);
        }

        public string Package;

        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            Package = Package.Replace(' ', '.');

            var applicationPath = WorkingDirectory + @"\" + Package;

            if (!_nPloyConfiguration.FileExists(applicationPath + @"\App_Install\Stop.ps1"))
                return 0;

            Console.WriteLine("Running stop scripts for package: " + Package);

            _powershellRunner.RunPowershellScript(@".\App_Install\Stop.ps1", applicationPath);
            return 0;
        }
    }
}