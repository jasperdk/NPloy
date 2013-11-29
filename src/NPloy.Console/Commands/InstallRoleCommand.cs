﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ManyConsole;

namespace NPloy.Commands
{
    public interface IInstallRoleCommand
    {
        int Run(string[] remainingArguments);
        string WorkingDirectory { get; set; }
        string Role { get; set; }
        string Environment { get; set; }
        string PackageSources { get; set; }
        string ConfigurationDirectory { get; set; }
        string NuGetPath { get; set; }
    }

    public class InstallRoleCommand : ConsoleCommand, IInstallRoleCommand
    {
        public InstallRoleCommand()
        {
            IsCommand("InstallRole", "InstallRole");
            HasAdditionalArguments(1, "Role");
            HasOption("d|directory=", "Deploy to this directory", s => WorkingDirectory = s);
            HasOption("e|environment=", "Deploy to this environment", s => Environment = s);
            HasOption("p|packagesources=", "NuGet packagesources", s => PackageSources = s);
            HasOption("c|configuration=", "NPloy configuration directory", s => ConfigurationDirectory = s);
            HasOption("n|nuget=", "NuGet console path", s => NuGetPath = s);
        }

        public string Role { get; set; }
        public string PackageSources { get; set; }
        public string Environment { get; set; }
        public string WorkingDirectory { get; set; }
        public string ConfigurationDirectory { get; set; }
        public string NuGetPath { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                var packages = new List<string>();
                var roleFile = @"roles\" + Role;
                if (!string.IsNullOrEmpty(ConfigurationDirectory))
                    roleFile = ConfigurationDirectory + @"\" + roleFile;

                string subFolder = "";
                if (Role.ToLower().EndsWith(".role") && File.Exists(roleFile))
                {
                    Console.WriteLine("Installing role: " + Role);
                    var doc = new XmlDocument();

                    doc.Load(roleFile);
                    var rootElement = doc.DocumentElement;
                    subFolder = rootElement.Attributes["subFolder"] != null ? rootElement.Attributes["subFolder"].Value : "";
                    var docPackages = doc.GetElementsByTagName("package");
                    foreach (XmlNode docPackage in docPackages)
                    {
                        Console.WriteLine("Package to install: " + docPackage.Attributes["id"].Value);
                        packages.Add(docPackage.Attributes["id"].Value);
                    }
                }
                else
                {
                    Console.WriteLine("Unknown role: " + Role);
                    return 1;
                }

                if (!string.IsNullOrEmpty(subFolder))
                    WorkingDirectory += @"\" + subFolder;

                InstallPackages(packages);
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

        private void StartPackages(IEnumerable<string> installedPackages)
        {
            var startPackageCommand = new StartPackageCommand();
            foreach (var installedPackage in installedPackages)
            {
                startPackageCommand.WorkingDirectory = WorkingDirectory;
                var exitCode = startPackageCommand.Run(new[] { installedPackage });
                if (exitCode > 0)
                    throw new ConsoleException(exitCode);
            }
        }

        private void InstallPackages(IEnumerable<string> packages)
        {
            foreach (var package in packages)
            {
                var installPackageCommand = new InstallPackageCommand();
                installPackageCommand.WorkingDirectory = WorkingDirectory;
                installPackageCommand.PackageSources = PackageSources;
                installPackageCommand.ConfigurationDirectory = ConfigurationDirectory;
                installPackageCommand.NuGetPath = NuGetPath;
                var exitCode = installPackageCommand.Run(new[] { package });
                if (exitCode > 0)
                    throw new ConsoleException(exitCode);
            }
        }
    }
}