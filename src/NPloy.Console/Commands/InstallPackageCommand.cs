using System;
using System.Collections.Generic;
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
            HasOption("e|environment=", "", e => Environment = e);
            HasOption("d|directory=", "Install in this directory", s => WorkingDirectory = s);
            HasOption("p|packagesources=", "Packagesources", s => PackageSources = s);
            HasOption("c|configuration=", "NPloy configuration directory", s => ConfigurationDirectory = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
            HasOption("v|version=", "Version of package to install ", s => Version = s);
            HasOption("o|verbose", "Verbose output", s => Verbose = s != null);
            HasOption("properties=", "Additional properties", s => Properties = s);
            HasOption("IncludePrerelease", "Include prerelease packages", s => IncludePrerelease = s != null);
        }

        public string Package { get; set; }
        public virtual string Environment { get; set; }
        public virtual string WorkingDirectory { get; set; }
        public virtual string PackageSources { get; set; }
        public virtual string ConfigurationDirectory { get; set; }
        public virtual string NuGetPath { get; set; }
        public virtual string Version { get; set; }
        public virtual bool Verbose { get; set; }
        public virtual string Properties { get; set; }
        public virtual bool IncludePrerelease { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                SetDefaultOptions();

                Console.WriteLine("Install package: " + Package);
                var installedPackages = NugetInstallPackage(Package, PackageSources);

                foreach (var installedPackage in installedPackages)
                {
                    RunInstallScripts(installedPackage);
                }

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void SetDefaultOptions()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = currentDirectory;
            if (string.IsNullOrEmpty(ConfigurationDirectory))
                ConfigurationDirectory = currentDirectory;
            //if (string.IsNullOrEmpty(NuGetPath))
            //    NuGetPath = ".nuget";
        }

        private void RunInstallScripts(string installedPackage)
        {
            var installedPackageScriptPath = Path.Combine(WorkingDirectory, installedPackage, @"App_Install");

            Console.WriteLine("Running install scripts in (" + installedPackageScriptPath + ") for package: " + Package);

            var files = _nPloyConfiguration.GetFiles(installedPackageScriptPath);
            foreach (
                var file in
                    files.Where(f => Path.GetFileName(f).ToLower().StartsWith("install"))
                    .OrderBy(n => n).ToArray()
                )
            {
                Console.WriteLine("Running install script : " + file);
                RunCommand(installedPackage, file);
            }

            using (StreamWriter sw = File.AppendText(Path.Combine(WorkingDirectory, "packages.config")))
            {
                sw.WriteLine(installedPackage);
            }
        }

        private void HandleArguments(string[] remainingArguments)
        {
            Package = remainingArguments[0].Replace(' ', '.');
        }

        private void RunCommand(string installedPackage, string file)
        {
            var environment = Environment ?? "dev";

            var properties = _nPloyConfiguration.GetProperties(Package, environment, ConfigurationDirectory);

            AddAdditionalProperties(properties);

            properties["Environment"] = environment;

            _nPloyConfiguration.SubstituteValues(properties);

            var argumentsString = "";
            foreach (var property in properties)
            {
                argumentsString += " -" + property.Key + @" """ + property.Value + @"""";
            }

            if (Verbose)
                Console.WriteLine("Running '{0}' script with arguments: {1}", file, argumentsString);
            _powershellRunner.RunPowershellScript(file, argumentsString, Path.Combine(WorkingDirectory, installedPackage));
        }

        private void AddAdditionalProperties(Dictionary<string, string> properties)
        {
            if (!string.IsNullOrEmpty(Properties))
            {
                var additionalProperties = Properties.Split(';');
                foreach (var additionalProperty in additionalProperties)
                {
                    var items = additionalProperty.Split('=');
                    var key = items[0].Replace(".", "");
                    if (!string.IsNullOrEmpty(key))
                    {
                        var value = additionalProperty.Remove(0, items[0].Length + 1);
                        properties[key] = value;
                    }
                }
            }
        }

        private IList<string> NugetInstallPackage(string package, string packageSources)
        {
            if (!string.IsNullOrEmpty(WorkingDirectory) && !Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);

            return _nuGetRunner.RunNuGetInstall(package, Version, packageSources, NuGetPath, WorkingDirectory, IncludePrerelease);
        }
    }
}