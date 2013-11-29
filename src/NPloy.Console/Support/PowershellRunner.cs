namespace NPloy.Support
{
    public interface IPowershellRunner
    {
        string RunPowershellScript(string script, string workingDirectory);
    }

    public class PowerShellRunner :  IPowershellRunner
    {
        private readonly ICommandRunner _commandRunner;

        public PowerShellRunner():this(new CommandRunner())
        {
        }

        public PowerShellRunner(ICommandRunner commandRunner)
        {
            _commandRunner = commandRunner;
        }

        public string RunPowershellScript(string script, string workingDirectory)
        {
            return _commandRunner.RunCommand(@"powershell", "-executionpolicy unrestricted " + script, workingDirectory);
        }
    }
}