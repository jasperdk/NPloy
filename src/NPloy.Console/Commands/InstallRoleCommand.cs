using System;
using System.Collections.Generic;
using System.IO;
using ManyConsole;
using NPloy.Support;

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
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly ICommandFactory _commandFactory;

        public InstallRoleCommand()
            : this(new NPloyConfiguration(), new CommandFactory())
        {

        }

        public InstallRoleCommand(INPloyConfiguration nPloyConfiguration, ICommandFactory commandFactory)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _commandFactory = commandFactory;
            IsCommand("InstallRole", "InstallRole");
            HasAdditionalArguments(1, "Role");
            HasOption("d|directory=", "Deploy to this directory", s => InstallDirectory = s);
            HasOption("e|environment=", "Deploy to this environment", s => Environment = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);
            HasOption("c|configuration=", "NPloy configuration directory", s => ConfigurationDirectory = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
            HasOption("s|start", "Start packages after install", s => AutoStart = s != null);
            HasOption("o|verbose", "Verbose output", s => Verbose = s != null);
            HasOption("properties=", "Additional properties", s => Properties = s);
            HasOption("IncludePrerelease", "Include prerelease packages", s => IncludePrerelease = s != null);
        }

        public string Role { get; set; }
        public virtual string PackageSources { get; set; }
        public virtual string Environment { get; set; }
        public virtual string InstallDirectory { get; set; }
        public virtual string ConfigurationDirectory { get; set; }
        public virtual string NuGetPath { get; set; }
        public virtual bool AutoStart { get; set; }
        public virtual bool Verbose { get; set; }
        public virtual string Properties { get; set; }
        public virtual bool IncludePrerelease { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                SetDefaultOptions();
                var packages = new List<string>();
                var roleFile = Path.Combine(ConfigurationDirectory, "roles", Role);

                if (!_nPloyConfiguration.FileExists(roleFile))
                {
                    Console.WriteLine("Unknown role: " + Role);
                    return 1;
                }

                Console.WriteLine("Installing role: " + Role);

                var roleConfig = _nPloyConfiguration.GetRoleConfig(roleFile);

                //This is not supported when uninstalling etc
                //if (!string.IsNullOrEmpty(roleConfig.SubFolder))
                //    InstallDirectory += @"\" + roleConfig.SubFolder;

                foreach (var packageConfig in roleConfig.Packages)
                {
                    Console.WriteLine("Package to install: " + packageConfig.Id);
                    var package = packageConfig.Id;
                    var version = packageConfig.Version;
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
            if (!Path.IsPathRooted(InstallDirectory))
                InstallDirectory = Path.Combine(Directory.GetCurrentDirectory(), InstallDirectory);
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
            var installPackageCommand = _commandFactory.GetCommand<InstallPackageCommand>();
            installPackageCommand.WorkingDirectory = InstallDirectory;
            installPackageCommand.PackageSources = PackageSources;
            installPackageCommand.ConfigurationDirectory = ConfigurationDirectory;
            installPackageCommand.NuGetPath = NuGetPath;
            installPackageCommand.Version = version;
            installPackageCommand.Environment = Environment;
            installPackageCommand.Verbose = Verbose;
            installPackageCommand.Properties = Properties;
            installPackageCommand.IncludePrerelease = IncludePrerelease;
            var exitCode = installPackageCommand.Run(new[] { package });
            if (exitCode > 0)
                throw new ConsoleException(exitCode);

        }
    }
}