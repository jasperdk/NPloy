using System;
using System.Collections.Generic;
using System.IO;
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
            if (Role.ToLower().EndsWith(".config") && File.Exists(Role))
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
                pProcess.WaitForExit();    
            }
            
            return 0;
        }
    }
}