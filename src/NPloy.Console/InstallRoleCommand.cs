using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using ManyConsole;

namespace NPloy
{
    public class InstallRoleCommand : ConsoleCommand
    {
        public InstallRoleCommand()
        {
            IsCommand("InstallRole", "InstallRole");
            HasAdditionalArguments(1, "Role");
            HasOption("d|directory=", "Deploy to this directory", s => WorkingDirectory = s);
        }

        public string Role;
        public string WorkingDirectory;

        public override int Run(string[] remainingArguments)
        {
            var packages = new List<string>();

            Role = remainingArguments[0];
            if (Role.ToLower().EndsWith(".role") && File.Exists(Role))
            {
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

            var installedPackages = new List<string>();
            foreach (var package in packages)
            {
                var pProcess = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        FileName = "nuget",
                        Arguments = string.Format("install {0}", package),
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                pProcess.StartInfo.WorkingDirectory = WorkingDirectory;

                pProcess.Start();
                string strOutput = pProcess.StandardOutput.ReadToEnd();
                Console.Write(strOutput);
                Match m = Regex.Match(strOutput, @"Successfully installed '([^']*)'\.", RegexOptions.CultureInvariant);
                if (m.Success)
                {
                    installedPackages.Add(m.Groups[1].Value);
                }
                pProcess.WaitForExit();
            }

            foreach (var installedPackage in installedPackages)
            {
                Console.WriteLine("Installed package: " + installedPackage);
                var installPackageCommand = new InstallPackageCommand();
                installPackageCommand.Run(new[] { installedPackage });
            }
            return 0;
        }
    }
}