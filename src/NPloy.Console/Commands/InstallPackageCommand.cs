using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public interface IInstallPackageCommand
    {
        string Package { get; set; }
    }

    public class InstallPackageCommand : ConsoleCommand, IInstallPackageCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly IPowershellRunner _powershellRunner;

        public InstallPackageCommand()
            : this(new NPloyConfiguration(),
                   new PowerShellRunner())
        {
        }

        public InstallPackageCommand(INPloyConfiguration nPloyConfiguration, IPowershellRunner powershellRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _powershellRunner = powershellRunner;
            IsCommand("InstallPackage", "InstallPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("environment", "", e => Environment = e);
            HasOption("d|directory=", "Install in this directory", s => WorkingDirectory = s);
            HasOption("p|packagesources=", "Packagesources", s => PackageSources = s);
        }

        public string Package { get; set; }
        public string Environment;
        public string WorkingDirectory { get; set; }
        public string PackageSources { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                Console.WriteLine("Install package: " + Package);
                var installedPackage = NugetInstallPackage(Package, PackageSources);

                Console.WriteLine("Running install scripts in (" + WorkingDirectory + @"\" + installedPackage + @"\App_Install" + ") for package: " + Package);

                string strOutput;
                var files = _nPloyConfiguration.GetFiles(WorkingDirectory + @"\" + installedPackage + @"\App_Install\");
                foreach (var file in files.Where(f => Path.GetFileName(f).ToLower().StartsWith("install")).ToArray())
                {
                    Console.WriteLine("Running install script : " + file);
                    RunCommand(installedPackage, @"App_Install\" + Path.GetFileName(file));
                    using (StreamWriter sw = File.AppendText(WorkingDirectory + @"\packages.config"))
                    {
                        sw.WriteLine(installedPackage);
                    }
                }

                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void HandleArguments(string[] remainingArguments)
        {
            Package = remainingArguments[0].Replace(' ', '.');
        }

        private string RunCommand(string installedPackage, string script)
        {
            var environment = Environment ?? "dev";
            var properties = _nPloyConfiguration.GetProperties(environment, Package);
            foreach (var property in properties)
            {
                script += " -" + property.Key + @" '" + property.Value + @"'";
            }

            var strOutput = _powershellRunner.RunPowershellScript(script, WorkingDirectory + @"\" + installedPackage);

            return strOutput;
        }

        private string NugetInstallPackage(string package, string packageSources)
        {
            var workingDirectory = WorkingDirectory;

            if (!string.IsNullOrEmpty(workingDirectory) && !Directory.Exists(workingDirectory))
                Directory.CreateDirectory(workingDirectory);

            var packageSourcesArgument = "";
            if (!string.IsNullOrEmpty(packageSources))
                packageSourcesArgument = @"-Source " + packageSources + "";

            var outputDirectoryArgument = "";
            if (!string.IsNullOrEmpty(workingDirectory))
                outputDirectoryArgument = @"-OutputDirectory " + workingDirectory + "";

            var installedPackage = "";

            var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @".nuget\nuget",
                    Arguments = string.Format(@"install {0} {1} {2}", package, outputDirectoryArgument, packageSourcesArgument),
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
                installedPackage = m.Groups[1].Value;
            }
            else
            {
                m = Regex.Match(strOutput, @"'([^']*)' already installed\.", RegexOptions.CultureInvariant);
                if (m.Success)
                {
                    installedPackage = m.Groups[1].Value;
                }
            }
            pProcess.WaitForExit();
            if (pProcess.ExitCode > 0)
                throw new ConsoleException(pProcess.ExitCode);

            return installedPackage.Replace(' ', '.');
        }

    }
}