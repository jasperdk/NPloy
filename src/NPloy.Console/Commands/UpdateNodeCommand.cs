using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class UpdateNodeCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly ICommandFactory _commandFactory;
        private readonly INuGetRunner _nuGetRunner;
        public const string PackageFileName = "packages.config";
        public UpdateNodeCommand()
            : this(new NPloyConfiguration(),
            new CommandFactory(),
            new NuGetRunner(new CommandRunner()))
        {
        }

        public UpdateNodeCommand(INPloyConfiguration nPloyConfiguration,
            ICommandFactory commandFactory,
            INuGetRunner nuGetRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _commandFactory = commandFactory;
            _nuGetRunner = nuGetRunner;
            IsCommand("UpdateNode", "UpdateNode");
            HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Install to this directory", s => InstallDirectory = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
            HasOption("s|start", "Start packages after install", s => AutoStart = s != null);
            HasOption("t|stop", "Stop all packages before update", s => FullStop = s != null);
            HasOption("o|verbose", "Verbose output", s => Verbose = s != null);
            HasOption("properties=", "Additional properties", s => Properties = s);
            HasOption("IncludePrerelease", "Include prerelease packages", s => IncludePrerelease = s != null);
            HasOption("r|remove", "Delete files and directories after uninstall", s => RemoveFilesAndDirectories = s != null);
        }

        public string Node;
        public string InstallDirectory;
        public string NuGetPath { get; set; }
        public string PackageSources { get; set; }
        public bool AutoStart { get; set; }
        public bool FullStop { get; set; }
        public bool Verbose { get; set; }
        public string Properties { get; set; }
        public bool IncludePrerelease { get; set; }
        public bool RemoveFilesAndDirectories { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                SetDefaultOptionValues();
                var packageFileName = Path.Combine(InstallDirectory, PackageFileName);

                if (!_nPloyConfiguration.FileExists(packageFileName))
                {
                    Console.WriteLine("Node has not been installed. Install or update!");
                    return -1;
                }

                HandleArguments(remainingArguments);

                if (!_nPloyConfiguration.FileExists(Node))
                {
                    Console.WriteLine("File not found: " + Node);
                    return 1;
                }

                Console.WriteLine("Updating node: " + Node);
                var nployConfigurationPath = Path.GetDirectoryName(Node);
                
                var installedPackages = _nPloyConfiguration.GetInstalledPackges(InstallDirectory).ToArray();

                var packageCandidates = new List<PackageConfig>();

                var roles = GetRolesFromFile();
                foreach (var role in roles)
                {
                    var roleFile = Path.Combine(nployConfigurationPath, "roles", role);
                    var roleConfig = _nPloyConfiguration.GetRoleConfig(roleFile);
                    foreach (var package in roleConfig.Packages)
                    {
                        var existingPackage = packageCandidates.SingleOrDefault(p => p.Id == package.Id);
                        if (existingPackage != null)
                        {
                            if (package.Version.CompareTo(existingPackage.Version) > 0)
                                continue;
                            packageCandidates.Remove(package);
                        }
                        packageCandidates.Add(package);
                    }
                }

                var packagesToInstall =
                    packageCandidates.Where(
                        pc =>
                        installedPackages.All(ip => !ip.Id.Equals(pc.Id, StringComparison.InvariantCultureIgnoreCase)))
                                     .ToList();
                var packagesToRemove =
                    installedPackages.Where(
                        ip =>
                        packageCandidates.All(pc => !ip.Id.Equals(pc.Id, StringComparison.InvariantCultureIgnoreCase)))
                                     .ToList();

                var packagesToUpdateCandidates =
                    installedPackages.Where(
                        ip =>
                        packageCandidates.Any(pc => ip.Id.Equals(pc.Id, StringComparison.InvariantCultureIgnoreCase)))
                                     .ToList();

                var packagesToUpdate = GetPackagesToUpdate(packagesToUpdateCandidates).ToList();

                if (!packagesToUpdate.Any())
                {
                    Console.WriteLine("No packages to update");
                    return 0;
                }

                if (FullStop)
                    StopNode();
                else
                    StopPackages(
                        packagesToRemove.Union(
                            installedPackages.Where(
                                p =>
                                packagesToUpdate.Any(
                                    pc => pc.Id.Equals(p.Id, StringComparison.InvariantCultureIgnoreCase)))));

                UninstallPackages(packagesToRemove.Union(
                            packagesToUpdateCandidates.Where(pc => packagesToUpdate.Any(p => p.Id == pc.Id))));
                InstallPackages(packagesToUpdate.Union(packagesToInstall), nployConfigurationPath);

                if (AutoStart)
                {
                    StartNode();
                }

                if (RemoveFilesAndDirectories)
                    DeleteDirectories(
                        packagesToRemove.Union(
                            packagesToUpdateCandidates.Where(pc => packagesToUpdate.Any(p => p.Id == pc.Id))));

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void UninstallPackages(IEnumerable<PackageConfig> packages)
        {
            foreach (var package in packages)
            {
                UninstallPackage(package);
            }
        }

        private void UninstallPackage(PackageConfig package)
        {
            var uninstallPackageCommand = _commandFactory.GetCommand<UninstallPackageCommand>();
            uninstallPackageCommand.WorkingDirectory = InstallDirectory;
            var exitCode = uninstallPackageCommand.Run(new[] { package.FullName });
            if (exitCode > 0)
                throw new ConsoleException(exitCode);

        }

        private void StopNode()
        {
            var stopNodeCommand = _commandFactory.GetCommand<StopNodeCommand>();
            stopNodeCommand.WorkingDirectory = InstallDirectory;
            stopNodeCommand.Run(new[] { Node });
        }

        private void StartNode()
        {
            var startNodeCommand = _commandFactory.GetCommand<StartNodeCommand>();
            startNodeCommand.WorkingDirectory = InstallDirectory;
            startNodeCommand.Run(new[] { Node });
        }

        private IEnumerable<PackageConfig> GetPackagesToUpdate(IEnumerable<PackageConfig> packagesToUpdateCandidates)
        {
            var nugetPackages = _nuGetRunner.RunNuGetList(PackageSources, NuGetPath, InstallDirectory, IncludePrerelease);

            foreach (var packagesToUpdateCandidate in packagesToUpdateCandidates)
            {
                if (packagesToUpdateCandidate.Version.CompareTo(nugetPackages[packagesToUpdateCandidate.Id]) < 0)
                    yield return new PackageConfig { Id = packagesToUpdateCandidate.Id, Version = nugetPackages[packagesToUpdateCandidate.Id] };
            }
        }

        private void InstallPackages(IEnumerable<PackageConfig> packages, string nployConfigurationPath)
        {
            foreach (var package in packages)
            {
                InstallPackage(package, nployConfigurationPath);
            }
        }

        private void InstallPackage(PackageConfig package, string nployConfigurationPath)
        {
            var installPackageCommand = _commandFactory.GetCommand<InstallPackageCommand>();
            installPackageCommand.WorkingDirectory = InstallDirectory;
            installPackageCommand.PackageSources = PackageSources;
            installPackageCommand.ConfigurationDirectory = nployConfigurationPath;
            installPackageCommand.NuGetPath = NuGetPath;
            installPackageCommand.Version = package.Version;
            installPackageCommand.Environment = GetEnvironmentFromFile();
            installPackageCommand.Verbose = Verbose;
            installPackageCommand.Properties = Properties;
            installPackageCommand.IncludePrerelease = IncludePrerelease;
            var exitCode = installPackageCommand.Run(new[] { package.Id });
            if (exitCode > 0)
                throw new ConsoleException(exitCode);

        }

        private void DeleteDirectories(IEnumerable<PackageConfig> packagesToRemove)
        {
            foreach (var package in packagesToRemove)
            {
                var pathToDelete = Path.Combine(InstallDirectory, package.FullName);
                if (Directory.Exists(pathToDelete))
                    Directory.Delete(pathToDelete, true);
            }
        }

        private void StopPackages(IEnumerable<PackageConfig> packages)
        {
            foreach (var package in packages)
            {
                var stopPackageCommand = _commandFactory.GetCommand<StopPackageCommand>();
                stopPackageCommand.WorkingDirectory = InstallDirectory;
                var result = stopPackageCommand.Run(new[] { package.FullName });
                if (result > 0)
                    throw new ConsoleException(result);
            }
        }

        private void SetDefaultOptionValues()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(InstallDirectory))
                InstallDirectory = currentDirectory;
            if (!Path.IsPathRooted(InstallDirectory))
                InstallDirectory = Path.Combine(Directory.GetCurrentDirectory(), InstallDirectory);
        }

        private string GetEnvironmentFromFile()
        {
            var nodeFileContent = _nPloyConfiguration.GetNodeXml(Node);

            if (nodeFileContent.Root != null)
                return nodeFileContent.Root.Attribute("environment").Value;
            return "";
        }

        private void HandleArguments(string[] remainingArguments)
        {
            Node = remainingArguments[0];
            if (!Node.ToLower().EndsWith(".node"))
                Node += ".node";
        }

        private IEnumerable<string> GetRolesFromFile()
        {
            var roles = new List<string>();

            var nodeFileContent = _nPloyConfiguration.GetNodeXml(Node);

            var docRoles = nodeFileContent.Elements().Where(e=>e.Name=="role");
            foreach (var docRole in docRoles)
            {
                roles.Add(docRole.Attribute("name").Value);
            }
            return roles;
        }
    }


}