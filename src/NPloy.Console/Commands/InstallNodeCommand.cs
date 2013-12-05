using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ManyConsole;

namespace NPloy.Commands
{
    public class InstallNodeCommand : ConsoleCommand
    {
        private readonly ICommandFactory _commandFactory;
        public const string PackageFileName = "packages.config";
        public InstallNodeCommand()
            : this(new CommandFactory())
        {
        }

        public InstallNodeCommand(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
            IsCommand("InstallNode", "InstallNode");
            HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Install to this directory", s => InstallDirectory = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
            HasOption("s|start", "Start packages after install", s => AutoStart = s != null);
        }

        public string Node;
        public string InstallDirectory;
        public string NuGetPath { get; set; }
        public string PackageSources { get; set; }
        public bool AutoStart { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                SetDefaultOptionValues();
                var packageFileName = Path.Combine(InstallDirectory, PackageFileName);

                if (File.Exists(packageFileName))
                {
                    Console.WriteLine("Node has already been installed. Uninstall or update!");
                    return -1;
                }

                HandleArguments(remainingArguments);

                if (!File.Exists(Node))
                {
                    Console.WriteLine("File not found: " + Node);
                    return 1;
                }

                Console.WriteLine("Installing node: " + Node);
                var nployConfigurationPath = Path.GetDirectoryName(Node);
                var roles = GetRolesFromFile();
                string environment = GetEnvironmentFromFile();
                foreach (var role in roles)
                {
                    var result = InstallRole(role, environment, nployConfigurationPath);
                    if (result > 0)
                        return result;
                }

                if (AutoStart)
                    StartNode();

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void StartNode()
        {
            var startNodeCommand = _commandFactory.GetCommand<StartNodeCommand>();
            startNodeCommand.WorkingDirectory = InstallDirectory;
            startNodeCommand.Run(new[] { Node });
        }

        private int InstallRole(string role, string environment, string nployConfigurationPath)
        {
            var installRoleCommand = _commandFactory.GetCommand<InstallRoleCommand>();
            installRoleCommand.InstallDirectory = InstallDirectory;
            installRoleCommand.Environment = environment;
            installRoleCommand.PackageSources = PackageSources;
            installRoleCommand.ConfigurationDirectory = nployConfigurationPath;
            installRoleCommand.NuGetPath = NuGetPath;
            var result = installRoleCommand.Run(new[] { role });
            return result;
        }

        private void SetDefaultOptionValues()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(InstallDirectory))
                InstallDirectory = currentDirectory;
        }

        private string GetEnvironmentFromFile()
        {
            var doc = new XmlDocument();
            doc.Load(Node);
            
            if (doc.DocumentElement != null)
                return doc.DocumentElement.Attributes["environment"].Value;
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
            var doc = new XmlDocument();
            doc.Load(Node);
            var docRoles = doc.GetElementsByTagName("role");
            foreach (XmlNode docRole in docRoles)
            {
                roles.Add(docRole.Attributes["name"].Value);
            }
            return roles;
        }
    }


}