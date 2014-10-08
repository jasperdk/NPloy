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
        public const string PackageFileName = "packages.config";
        public UpdateNodeCommand()
            : this(new NPloyConfiguration(),
            new CommandFactory())
        {
        }

        public UpdateNodeCommand(INPloyConfiguration nPloyConfiguration, ICommandFactory commandFactory)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _commandFactory = commandFactory;
            IsCommand("UpdateNode", "UpdateNode");
            HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Install to this directory", s => InstallDirectory = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
            HasOption("s|start", "Start packages after install", s => AutoStart = s != null);
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
                var roles = GetRolesFromFile();
                string environment = GetEnvironmentFromFile();

                var installedPackages = _nPloyConfiguration.GetInstalledPackges(InstallDirectory).ToArray();
                
                var packageCandidates = new List<PackageConfig>();

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

                var packagesToInstall = packageCandidates.Where(pc=>installedPackages.All(ip => ip != pc.FullName)).ToList();
                var packagesToRemove = installedPackages.Where(ip => packageCandidates.All(pc => ip != pc.FullName)).ToList();

                var packagesToUpdateCandidates = packageCandidates.Where(pc => installedPackages.Any(ip => ip == pc.FullName)).ToList();

                var packagesToUpdate = GetPackagesToUpdate(packagesToUpdateCandidates).ToList();
                
                StopPackages(packagesToRemove.Union(packagesToUpdate.Select(p=>p.FullName)));

                InstallPackages(packagesToUpdate.Union(packagesToInstall), nployConfigurationPath);
                
                if (AutoStart)
                    StartPackages(packagesToUpdate.Union(packagesToInstall));

                if (RemoveFilesAndDirectories)
                    DeleteDirectories(
                        packagesToRemove.Union(
                            packagesToUpdateCandidates.Where(pc => packagesToUpdate.Any(p => p.Id == pc.Id)).Select(p=>p.FullName)));

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private IEnumerable<PackageConfig> GetPackagesToUpdate(IEnumerable<PackageConfig> packagesToUpdateCandidates)
        {
            throw new NotImplementedException();
            var nugetPackages = new Dictionary<string,string>(); //TODO get available nuget packages
            foreach (var packagesToUpdateCandidate in packagesToUpdateCandidates)
            {
                if (packagesToUpdateCandidate.Version.CompareTo(nugetPackages[packagesToUpdateCandidate.Id])<0)
                    yield return new PackageConfig { Id = packagesToUpdateCandidate.Id, Version = nugetPackages[packagesToUpdateCandidate.Id] };
            }
        }

        private void InstallPackages(IEnumerable<PackageConfig> packages, string nployConfigurationPath)
        {
            foreach (var package in packages)
            {
                InstallPackage(package,nployConfigurationPath);
            }
        }

        private void InstallPackage(PackageConfig package,string nployConfigurationPath)
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

        private void DeleteDirectories(IEnumerable<string> packagesToRemove)
        {
            foreach (var package in packagesToRemove)
            {
                var pathToDelete = Path.Combine(InstallDirectory, package);
                if (Directory.Exists(pathToDelete))
                    Directory.Delete(pathToDelete, true);
            }
        }

        private void StartPackages(IEnumerable<PackageConfig> packages)
        {
            foreach (var package in packages)
            {
                var startPackageCommand = _commandFactory.GetCommand<StartPackageCommand>();
                startPackageCommand.WorkingDirectory = InstallDirectory;
                var result = startPackageCommand.Run(new[] { package.Id });
                if (result > 0)
                        throw new ConsoleException(result);

            }
        }

        private void StopPackages(IEnumerable<string> packages)
        {
            foreach (var package in packages)
            {
                var stopPackageCommand = _commandFactory.GetCommand<StopPackageCommand>();
                stopPackageCommand.WorkingDirectory = InstallDirectory;
                var result = stopPackageCommand.Run(new[] { package });
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

            if (nodeFileContent.DocumentElement != null)
                return nodeFileContent.DocumentElement.Attributes["environment"].Value;
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

            var docRoles = nodeFileContent.GetElementsByTagName("role");
            foreach (XmlNode docRole in docRoles)
            {
                roles.Add(docRole.Attributes["name"].Value);
            }
            return roles;
        }
    }


}