using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ManyConsole;

namespace NPloy.Commands
{
    public interface IInstallRoleCommand
    {
        int Run(string[] remainingArguments);
        string InstallDirectory { get; set; }
        string Role { get; set; }
        string Environment { get; set; }
        string PackageSources { get; set; }
        string ConfigurationDirectory { get; set; }
        string NuGetPath { get; set; }
        bool AutoStart { get; set; }
    }

    public class InstallRoleCommand : ConsoleCommand, IInstallRoleCommand
    {
        public InstallRoleCommand()
        {
            IsCommand("InstallRole", "InstallRole");
            HasAdditionalArguments(1, "Role");
            HasOption("d|directory=", "Deploy to this directory", s => InstallDirectory = s);
            HasOption("e|environment=", "Deploy to this environment", s => Environment = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);
            HasOption("c|configuration=", "NPloy configuration directory", s => ConfigurationDirectory = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
            HasOption("s|start", "Start packages after install", s => AutoStart = s != null);
        }

        public string Role { get; set; }
        public virtual string PackageSources { get; set; }
        public virtual string Environment { get; set; }
        public virtual string InstallDirectory { get; set; }
        public virtual string ConfigurationDirectory { get; set; }
        public virtual string NuGetPath { get; set; }
        public virtual bool AutoStart { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                SetDefaultOptions();
                var packages = new List<string>();
                var roleFile = Path.Combine(ConfigurationDirectory, "roles", Role);

                if (!File.Exists(roleFile))
                {
                    Console.WriteLine("Unknown role: " + Role);
                    return 1;
                }

                Console.WriteLine("Installing role: " + Role);
                var doc = new XmlDocument();

                doc.Load(roleFile);
                var rootElement = doc.DocumentElement;
                var subFolder = rootElement.Attributes["subFolder"] != null ? rootElement.Attributes["subFolder"].Value : "";
                if (!string.IsNullOrEmpty(subFolder))
                    InstallDirectory += @"\" + subFolder;
                var docPackages = doc.GetElementsByTagName("package");
                foreach (XmlNode docPackage in docPackages)
                {
                    Console.WriteLine("Package to install: " + docPackage.Attributes["id"].Value);
                    var package = docPackage.Attributes["id"].Value;
                    var version = docPackage.Attributes["version"] != null ? docPackage.Attributes["version"].Value : null;
                    packages.Add(package);
                    InstallPackage(package, version);
                }

                if (AutoStart)
                    StartPackages(packages);
                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void HandleArguments(string[] remainingArguments)
        {
            Role = remainingArguments[0];
        }

        private void SetDefaultOptions()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(InstallDirectory))
                InstallDirectory = currentDirectory;
            if (string.IsNullOrEmpty(ConfigurationDirectory))
                ConfigurationDirectory = currentDirectory;
        }

        private void StartPackages(IEnumerable<string> installedPackages)
        {
            var startPackageCommand = new StartPackageCommand();
            foreach (var installedPackage in installedPackages)
            {
                startPackageCommand.WorkingDirectory = InstallDirectory;
                var exitCode = startPackageCommand.Run(new[] { installedPackage });
                if (exitCode > 0)
                    throw new ConsoleException(exitCode);
            }
        }

        private void InstallPackage(string package, string version)
        {
            var installPackageCommand = new InstallPackageCommand();
            installPackageCommand.WorkingDirectory = InstallDirectory;
            installPackageCommand.PackageSources = PackageSources;
            installPackageCommand.ConfigurationDirectory = ConfigurationDirectory;
            installPackageCommand.NuGetPath = NuGetPath;
            installPackageCommand.Version = version;
            var exitCode = installPackageCommand.Run(new[] { package });
            if (exitCode > 0)
                throw new ConsoleException(exitCode);

        }
    }
}