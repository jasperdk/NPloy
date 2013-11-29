using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly INuGetRunner _nuGetRunner;

        public InstallPackageCommand()
            : this(new NPloyConfiguration(),
                   new PowerShellRunner(),
            new NuGetRunner())
        {
        }

        public InstallPackageCommand(INPloyConfiguration nPloyConfiguration, IPowershellRunner powershellRunner, INuGetRunner nuGetRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _powershellRunner = powershellRunner;
            _nuGetRunner = nuGetRunner;
            IsCommand("InstallPackage", "InstallPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("environment", "", e => Environment = e);
            HasOption("d|directory=", "Install in this directory", s => WorkingDirectory = s);
            HasOption("p|packagesources=", "Packagesources", s => PackageSources = s);
            HasOption("c|configuration=", "NPloy configuration directory", s => ConfigurationDirectory = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
        }

        public string Package { get; set; }
        public string Environment;
        public string WorkingDirectory { get; set; }
        public string PackageSources { get; set; }
        public string ConfigurationDirectory { get; set; }
        public string NuGetPath { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                Console.WriteLine("Install package: " + Package);
                var installedPackage = NugetInstallPackage(Package, PackageSources);

                Console.WriteLine("Running install scripts in (" + WorkingDirectory + @"\" + installedPackage + @"\App_Install" + ") for package: " + Package);

                string strOutput;
                var files = _nPloyConfiguration.GetFiles(WorkingDirectory + @"\" + installedPackage + @"\App_Install\");
                foreach (var file in files.Where(f => Path.GetFileName(f).ToLower().StartsWith("install")).OrderBy(n => n).ToArray())
                {
                    Console.WriteLine("Running install script : " + file);
                    RunCommand(installedPackage, @"App_Install\" + Path.GetFileName(file));
                }

                using (StreamWriter sw = File.AppendText(WorkingDirectory + @"\packages.config"))
                {
                    sw.WriteLine(installedPackage);
                }

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

        private string RunCommand(string installedPackage, string script)
        {
            var environment = Environment ?? "dev";
            var properties = _nPloyConfiguration.GetProperties(Package, environment,ConfigurationDirectory);
            foreach (var property in properties)
            {
                script += " -" + property.Key + @" '" + property.Value + @"'";
            }

            var strOutput = _powershellRunner.RunPowershellScript(script, WorkingDirectory + @"\" + installedPackage);

            return strOutput;
        }

        private string NugetInstallPackage(string package, string packageSources)
        {
            if (!string.IsNullOrEmpty(WorkingDirectory) && !Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);

            return _nuGetRunner.RunNuGetInstall(package, packageSources, NuGetPath, WorkingDirectory);
        }
    }
}