using System.Text.RegularExpressions;

namespace NPloy.Support
{
    public interface INuGetRunner
    {
        string RunNuGetInstall(string package, string packageSources, string nugetPath, string workingDirectory);
    }

    public class NuGetRunner : INuGetRunner
    {
        private readonly ICommandRunner _commandRunner;

        public NuGetRunner()
            : this(new CommandRunner())
        {

        }

        public NuGetRunner(ICommandRunner commandRunner)
        {
            _commandRunner = commandRunner;
        }

        public string RunNuGetInstall(string package, string packageSources, string nugetPath, string workingDirectory)
        {
            var packageSourcesArgument = "";
            if (!string.IsNullOrEmpty(packageSources))
                packageSourcesArgument = @"-Source " + packageSources + "";

            var outputDirectoryArgument = "";
            if (!string.IsNullOrEmpty(workingDirectory))
                outputDirectoryArgument = @"-OutputDirectory " + workingDirectory + "";

            if (!string.IsNullOrEmpty(nugetPath))
                nugetPath += @"\";

            var installedPackage = "";

            var strOutput = _commandRunner.RunCommand(nugetPath + @"nuget", string.Format(@"install {0} {1} {2}", package, outputDirectoryArgument, packageSourcesArgument), workingDirectory);

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
                else
                {
                    //Unknown result
                    throw new ConsoleException(2);
                }
            }

            return installedPackage.Replace(' ', '.');
        }
    }
}