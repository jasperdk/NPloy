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
    }

    public class InstallRoleCommand : ConsoleCommand, IInstallRoleCommand
    {
        public InstallRoleCommand()
        {
            IsCommand("InstallRole", "InstallRole");
            HasAdditionalArguments(1, "Role");
            HasOption("d|directory=", "Deploy to this directory", s => WorkingDirectory = s);
        }

        public string Role;
        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            var packages = new List<string>();

            Role = remainingArguments[0];
            if (Role.ToLower().EndsWith(".role") && File.Exists(Role))
            {
                Console.WriteLine("Installing role: " + Role);
                var doc = new XmlDocument();
                doc.Load(Role);
                var docPackages = doc.GetElementsByTagName("package");
                foreach (XmlNode docPackage in docPackages)
                {
                    packages.Add(docPackage.Attributes["id"].Value);
                }

            }
            else
            {
                packages.Add(Role);
            }

            var installedPackages = NugetInstallPackages(packages);
            InstallPackages(installedPackages);
            StartPackages(installedPackages);

            return 0;
        }

        private List<string> NugetInstallPackages(IEnumerable<string> packages)
        {
            var installedPackages = new List<string>();
            foreach (var package in packages)
            {
                var pProcess = new System.Diagnostics.Process
                    {
                        StartInfo =
                            {
                                FileName = "nuget",
                                Arguments = string.Format(@"install {0} -OutputDirectory {1} -Source d:\install\packages", package, WorkingDirectory),
                                UseShellExecute = false,
                                RedirectStandardOutput = true
                            }
                    };

                pProcess.StartInfo.WorkingDirectory = WorkingDirectory;

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
                if (pProcess.ExitCode > 1)
                    throw new ConsoleException(pProcess.ExitCode);
            }
            return installedPackages;
        }

        private void StartPackages(IEnumerable<string> installedPackages)
        {
            foreach (var installedPackage in installedPackages)
            {
                var startPackageCommand = new StartPackageCommand();
                startPackageCommand.WorkingDirectory = WorkingDirectory;
                startPackageCommand.Package = installedPackage;
                var exitCode = startPackageCommand.Run(new[] { installedPackage });
                if (exitCode != 0)
                    throw new ConsoleException(exitCode);
            }
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
                using (StreamWriter sw = File.AppendText(WorkingDirectory+@"\packages.config"))
                {
                    sw.WriteLine(installedPackage);
                }
            }
        }
    }
}