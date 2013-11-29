using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
    }

    public class InstallRoleCommand : ConsoleCommand, IInstallRoleCommand
    {
        public InstallRoleCommand()
        {
            IsCommand("InstallRole", "InstallRole");
            HasAdditionalArguments(1, "Role");
            HasOption("d|directory=", "Deploy to this directory", s => WorkingDirectory = s);
            HasOption("e|environment=", "Deploy to this directory", s => Environment = s);
            HasOption("p|packagesources=", "Packagesources", s => PackageSources = s);
        }

        public string Role { get; set; }
        public string PackageSources { get; set; }
        public string Environment { get; set; }
        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            HandleArguments(remainingArguments);
            var packages = new List<string>();
            var roleFile = @".nploy\roles\" + Role;
            string subFolder = "";

            if (Role.ToLower().EndsWith(".role") && File.Exists(roleFile))
            {
                Console.WriteLine("Installing role: " + Role);
                var doc = new XmlDocument();

                doc.Load(roleFile);
                var rootElement = doc.DocumentElement;
                subFolder = rootElement.Attributes["subFolder"]!=null?rootElement.Attributes["subFolder"].Value:"";
                var docPackages = doc.GetElementsByTagName("package");
                foreach (XmlNode docPackage in docPackages)
                {
                    Console.WriteLine("Package to install: " + docPackage.Attributes["id"].Value);
                    packages.Add(docPackage.Attributes["id"].Value);
                }
            }
            else
            {
                Console.WriteLine("Package to install: " + Role);
                packages.Add(Role);
            }

            var installedPackages = NugetInstallPackages(packages,subFolder, PackageSources);
            if (string.IsNullOrEmpty(subFolder))
            {
                InstallPackages(installedPackages);
                StartPackages(installedPackages);
            }

            return 0;
        }

        private void HandleArguments(string[] remainingArguments)
        {
            Role = remainingArguments[0];
        }

        private List<string> NugetInstallPackages(IEnumerable<string> packages, string subFolder, string packageSources)
        {
            var workingDirectory = WorkingDirectory;
            if (!string.IsNullOrEmpty(subFolder))
                workingDirectory += @"\" + subFolder;
            if (!Directory.Exists(workingDirectory))
                Directory.CreateDirectory(workingDirectory);

            var packageSourcesArgument = "";
            if (!string.IsNullOrEmpty(packageSources))
                packageSourcesArgument = @"-Source " + packageSources + "";
            var installedPackages = new List<string>();
            foreach (var package in packages)
            {
                var pProcess = new System.Diagnostics.Process
                    {
                        StartInfo =
                            {
                                FileName = @".nuget\nuget",
                                Arguments = string.Format(@"install {0} -OutputDirectory {1} {2}", package, workingDirectory, packageSourcesArgument),
                                UseShellExecute = false,
                                RedirectStandardOutput = true
                            }
                    };

                pProcess.StartInfo.WorkingDirectory = workingDirectory;

                pProcess.Start();
                var strOutput = pProcess.StandardOutput.ReadToEnd();
                Console.Write(strOutput);
                var m = Regex.Match(strOutput, @"Successfully installed '([^']*)'\.", RegexOptions.CultureInvariant);
                if (m.Success)
                {
                    installedPackages.Add(m.Groups[1].Value);
                }
                else
                {
                    m = Regex.Match(strOutput, @"'([^']*)' already installed\.", RegexOptions.CultureInvariant);
                    if (m.Success)
                    {
                        installedPackages.Add(m.Groups[1].Value);
                    }
                }
                pProcess.WaitForExit();
                if (pProcess.ExitCode > 0)
                    throw new ConsoleException(pProcess.ExitCode);
            }
            return installedPackages;
        }

        private void StartPackages(IEnumerable<string> installedPackages)
        {
            var startNodeCommand = new StartNodeCommand();
            startNodeCommand.WorkingDirectory = WorkingDirectory;
            //startNodeCommand.Packages = installedPackage;
            var exitCode = startNodeCommand.Run(new string[0]);
            if (exitCode != 0)
                throw new ConsoleException(exitCode);
        }

        private void InstallPackages(IEnumerable<string> installedPackages)
        {
            foreach (var installedPackage in installedPackages)
            {
                var installPackageCommand = new InstallPackageCommand();
                installPackageCommand.WorkingDirectory = WorkingDirectory;
                installPackageCommand.Package = installedPackage;
                var exitCode = installPackageCommand.Run(new string[0]);
                if (exitCode != 0)
                    throw new ConsoleException(exitCode);
                using (StreamWriter sw = File.AppendText(WorkingDirectory + @"\packages.config"))
                {
                    sw.WriteLine(installedPackage);
                }
            }
        }
    }
}