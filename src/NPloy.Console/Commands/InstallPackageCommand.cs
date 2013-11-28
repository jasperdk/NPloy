using System;
using System.IO;
using System.Linq;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public interface IInstallPackageCommand
    {
        string Package { get; set; }
    }

    public class InstallPackageCommand : ConsoleCommand, IInstallPackageCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly IPowershellRunner _powershellRunner;

        public InstallPackageCommand()
            : this(new NPloyConfiguration(),
            new PowerShellRunner())
        {
        }

        public InstallPackageCommand(INPloyConfiguration nPloyConfiguration,IPowershellRunner powershellRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _powershellRunner = powershellRunner;
            IsCommand("InstallPackage", "InstallPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("environment", "", e => Environment = e);
            HasOption("d|directory=", "Install in this directory", s => WorkingDirectory = s);
        }

        public string Package { get; set; }
        public string Environment;
        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            if (string.IsNullOrEmpty(Package))
                throw new ApplicationException("Package must be set!");
            Package = Package.Replace(' ', '.');
            Console.WriteLine("Running install scripts in (" + WorkingDirectory + @"\" + Package + @"\App_Install" + ") for package: " + Package);

            string strOutput;
            var files = _nPloyConfiguration.GetFiles(WorkingDirectory + @"\" + Package + @"\App_Install\");
            foreach (var file in files.Where(f => Path.GetFileName(f).ToLower().StartsWith("install")).ToArray())
            {
                Console.WriteLine("Running install script : " + file);
                RunCommand(@"App_Install\" + Path.GetFileName(file));
            }

            return 0;
        }

        private string RunCommand(string script)
        {
            var environment = Environment ?? "dev";
            var properties = _nPloyConfiguration.GetProperties(environment);
            foreach (var property in properties)
            {
               script += " -" + property.Key + @" '" +property.Value +@"'";
            }

            Package = Package.Replace(' ', '.');

            var strOutput = _powershellRunner.RunPowershellScript(script, WorkingDirectory + @"\" + Package);

            return strOutput;
        }
    }
}