﻿using System;
using System.IO;
using ManyConsole;
using NPloy.Support;

namespace NPloy.Commands
{
    public class StopPackageCommand : ConsoleCommand
    {
        private readonly INPloyConfiguration _nPloyConfiguration;
        private readonly IPowershellRunner _powershellRunner;

        public StopPackageCommand()
            : this(new NPloyConfiguration(), new PowerShellRunner())
        {

        }

        public StopPackageCommand(INPloyConfiguration nPloyConfiguration,
            IPowershellRunner powershellRunner)
        {
            _nPloyConfiguration = nPloyConfiguration;
            _powershellRunner = powershellRunner;
            IsCommand("StopPackage", "StopPackage");
            HasAdditionalArguments(1, "InstalledPackage");
            HasOption("d|directory=", "Stop in this directory", s => WorkingDirectory = s);
        }

        public string InstalledPackage;

        public string WorkingDirectory { get; set; }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                HandleArguments(remainingArguments);
                SetDefaultOptionValues();
                Console.WriteLine("Stop package: " + InstalledPackage);

                var applicationPath = WorkingDirectory + @"\" + InstalledPackage;

                var stopScript = applicationPath + @"\App_Install\Stop.ps1";
                if (!_nPloyConfiguration.FileExists(stopScript))
                    return 0;

                Console.WriteLine("Running stop scripts for package: " + InstalledPackage);

                _powershellRunner.RunPowershellScript(stopScript, null, applicationPath);
                return 0;
            }
            catch (ConsoleException c)
            {
                return c.ExitCode;
            }
        }

        private void HandleArguments(string[] remainingArguments)
        {
            InstalledPackage = remainingArguments[0].Replace(' ', '.');
        }

        private void SetDefaultOptionValues()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = currentDirectory;
            if (!Path.IsPathRooted(WorkingDirectory))
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), WorkingDirectory);
        }
    }
}