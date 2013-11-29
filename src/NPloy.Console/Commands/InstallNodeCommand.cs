﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ManyConsole;

namespace NPloy.Commands
{
    public class InstallNodeCommand : ConsoleCommand
    {
        public const string PackageFileName = "packages.config";
        public InstallNodeCommand()
            : this(new InstallRoleCommand())
        {
        }

        public InstallNodeCommand(IInstallRoleCommand installRoleCommand)
        {
            _installRoleCommand = installRoleCommand;
            IsCommand("InstallNode", "InstallNode");
            HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Install to this directory", s => WorkingDirectory = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);

        }

        protected string PackageSources { get; set; }
        public string Node;
        public string WorkingDirectory;
        private readonly IInstallRoleCommand _installRoleCommand;

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var packageFileName = !string.IsNullOrEmpty(WorkingDirectory) ? WorkingDirectory + @"\" + PackageFileName : PackageFileName;

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
                    _installRoleCommand.WorkingDirectory = WorkingDirectory;
                    _installRoleCommand.Environment = environment;
                    _installRoleCommand.PackageSources = PackageSources;
                    _installRoleCommand.ConfigurationDirectory = nployConfigurationPath;
                    var result = _installRoleCommand.Run(new[] { role });
                    if (result > 0)
                        return result;
                }

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private string GetEnvironmentFromFile()
        {
            var doc = new XmlDocument();
            doc.Load(Node);
            var docNodes = doc.GetElementsByTagName("node");
            var node = docNodes.Item(0);

            if (node != null)
                return node.Attributes["environment"].Value;
            return "";
        }

        private void HandleArguments(string[] remainingArguments)
        {
            _installRoleCommand.WorkingDirectory = WorkingDirectory;
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