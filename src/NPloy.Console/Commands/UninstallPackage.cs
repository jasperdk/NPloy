using System;
using System.IO;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class UninstallPackageCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly IPowershellRunner _powershellRunner;

        public UninstallPackageCommand()
            : this(new NPloyConfiguration(), new PowerShellRunner())
        {
        }

        public UninstallPackageCommand(INPloyConfiguration nPloyConfiguration,
            IPowershellRunner powershellRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _powershellRunner = powershellRunner;
            IsCommand("UninstallPackage", "UninstallPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("d|directory=", "Uninstall from this directory", s => WorkingDirectory = s);
        }

        public string Package;

        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                SetDefaultOptionValues();
                Console.WriteLine("Uninstall package: " + Package);

                var applicationPath = Path.Combine(WorkingDirectory, Package);

                if (!_nPloyConfiguration.FileExists(applicationPath + @"\App_Install\Uninstall.ps1"))
                    return 0;

                Console.WriteLine("Running uninstall scripts for package: " + Package);

                _powershellRunner.RunPowershellScript(@".\App_Install\Uninstall.ps1", null, applicationPath);
                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void HandleArguments(string[] remainingArguments)
        {
            Package = remainingArguments[0].Replace(' ', '.');
        }

        private void SetDefaultOptionValues()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = currentDirectory;
        }
    }
}