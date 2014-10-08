using System;
using System.IO;

namespace NPloy.Support
{
    public interface IPowershellRunner
    {
        string RunPowershellScript(string file, string arguments, string workingDirectory);
    }

    public class PowerShellRunner : IPowershellRunner
    {
        private readonly ICommandRunner _commandRunner;

        public PowerShellRunner()
            : this(new CommandRunner())
        {
        }

        public PowerShellRunner(ICommandRunner commandRunner)
        {
            _commandRunner = commandRunner;
        }

        public string RunPowershellScript(string file, string arguments, string workingDirectory)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine("Executing powershell script: File not found:" + file);
                return "";
            }

            var powershellArguments = "-executionpolicy unrestricted -Noninteractive -file " + file + " " + arguments;
            return _commandRunner.RunCommand(@"powershell", powershellArguments, workingDirectory);
        }
    }
}