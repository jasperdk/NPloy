using System;
using System.IO;
using System.Linq;
using ManyConsole;

namespace NPloy.Commands
{
    public class InstallPackageCommand : ConsoleCommand
    {
        public InstallPackageCommand()
        {
            IsCommand("InstallPackage", "InstallPackage");
            HasAdditionalArguments(1, "Package");
            HasOption("environment", "", e => Environment = e);
            HasOption("d|directory=", "Install in this directory", s => WorkingDirectory = s);
        }

        public string Package;
        public string Environment;
        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            Package = Package.Replace(' ', '.');
            Console.WriteLine("Running install scripts in (" + WorkingDirectory + @"\" + Package + @"\App_Install" + ") for package: " + Package);

            string strOutput;
            var files = Directory.GetFiles(WorkingDirectory + @"\" + Package + @"\App_Install\");
            foreach (var file in files.Where(f => Path.GetFileName(f).ToLower().StartsWith("install")).ToArray())
            {
                Console.WriteLine("Running install script : " + file);
                RunCommand(@"App_Install\" + Path.GetFileName(file));
            }

            return 0;
        }

        private string RunCommand(string script)
        {
            var environment = Environment ?? "dev";
            var properties = File.ReadAllLines(@".nploy\environments\" + environment + @"\env.prop");
            foreach (var property in properties)
            {
                var items = property.Split('=');
                script += " -" + items[0].Replace(".", "") + @" '" + property.Remove(0, items[0].Length + 1) + @"'";
            }

            Package = Package.Replace(' ', '.');
            
            var pProcess = new System.Diagnostics.Process
                {
                    StartInfo =
                        {
                            FileName = @"powershell",
                            Arguments = script,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            WorkingDirectory = WorkingDirectory + @"\" + Package
                        }
                };

            
            pProcess.Start();
            var strOutput = pProcess.StandardOutput.ReadToEnd();
            Console.Write(strOutput);
            pProcess.WaitForExit();
            if (pProcess.ExitCode != 0)
                throw new ConsoleException(pProcess.ExitCode);

            return strOutput;
        }
    }
}