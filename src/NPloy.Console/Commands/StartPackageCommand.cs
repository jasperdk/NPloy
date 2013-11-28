using System;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class StartPackageCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly IPowershellRunner _powershellRunner;

        public StartPackageCommand()
            : this(new NPloyConfiguration(), new PowerShellRunner())
        {

        }

        public StartPackageCommand(INPloyConfiguration nPloyConfiguration,
            IPowershellRunner powershellRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _powershellRunner = powershellRunner;
            IsCommand("StartPackage", "StartPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("d|directory=", "Start from this directory", s => WorkingDirectory = s);
        }

        public string Package;

        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            Package = Package.Replace(' ', '.');

            var applicationPath = WorkingDirectory + @"\" + Package;

            if (!_nPloyConfiguration.FileExists(applicationPath + @"\App_Install\Start.ps1"))
                return 0;

            Console.WriteLine("Running start scripts for package: " + Package);

            _powershellRunner.RunPowershellScript(@".\App_Install\Start.ps1", applicationPath);
            return 0;
        }
    }
}