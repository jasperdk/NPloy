using System;
using System.IO;
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
            HasAdditionalArguments(1, "InstalledPackage");
            HasOption("d|directory=", "Start from this directory", s => WorkingDirectory = s);
        }

        public string InstalledPackage;

        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                SetDefaultOptionValues();
                Console.WriteLine("Start package: " + InstalledPackage);

                var applicationPath = Path.Combine(WorkingDirectory ,InstalledPackage);

                if (!_nPloyConfiguration.FileExists(applicationPath + @"\App_Install\Start.ps1"))
                    return 0;

                Console.WriteLine("Running start scripts for package: " + InstalledPackage);

                _powershellRunner.RunPowershellScript(@".\App_Install\Start.ps1", null, applicationPath);
                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void HandleArguments(string[] remainingArguments)
        {
            InstalledPackage = remainingArguments[0].Replace(' ', '.');
        }

        private void SetDefaultOptionValues()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = currentDirectory;
        }
    }
}