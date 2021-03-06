﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class InstallNodeCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly ICommandFactory _commandFactory;
        public const string PackageFileName = "packages.config";
        public InstallNodeCommand()
            : this(new NPloyConfiguration(),
            new CommandFactory())
        {
        }

        public InstallNodeCommand(INPloyConfiguration nPloyConfiguration, ICommandFactory commandFactory)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _commandFactory = commandFactory;
            IsCommand("InstallNode", "InstallNode");
            HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Install to this directory", s => InstallDirectory = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
            HasOption("s|start", "Start packages after install", s => AutoStart = s != null);
            HasOption("o|verbose", "Verbose output", s => Verbose = s != null);
            HasOption("properties=", "Additional properties", s => Properties = s);
            HasOption("IncludePrerelease", "Include prerelease packages", s => IncludePrerelease = s != null);
        }

        public string Node;
        public string InstallDirectory;
        public string NuGetPath { get; set; }
        public string PackageSources { get; set; }
        public bool AutoStart { get; set; }
        public bool Verbose { get; set; }
        public string Properties { get; set; }
        public bool IncludePrerelease { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                SetDefaultOptionValues();
                var packageFileName = Path.Combine(InstallDirectory, PackageFileName);

                if (_nPloyConfiguration.FileExists(packageFileName))
                {
                    Console.WriteLine("Node has already been installed. Uninstall or update!");
                    return -1;
                }

                HandleArguments(remainingArguments);

                if (!_nPloyConfiguration.FileExists(Node))
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
            installRoleCommand.Verbose = Verbose;
            installRoleCommand.Properties = Properties;
            installRoleCommand.IncludePrerelease = IncludePrerelease;
            var result = installRoleCommand.Run(new[] { role });
            return result;
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
            var nodeFileContent = _nPloyConfiguration.GetNodeXml(Node);

            return nodeFileContent.Descendants("role").Select(role => role.Attribute("name").Value).ToList();
        }
    }


}