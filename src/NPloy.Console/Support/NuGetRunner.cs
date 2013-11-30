using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPloy.Support
{
    public interface INuGetRunner
    {
        IList<string> RunNuGetInstall(string package, string packageSources, string nugetPath, string workingDirectory);
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

        public IList<string> RunNuGetInstall(string package, string packageSources, string nugetPath, string workingDirectory)
        {
            var packageSourcesArgument = "";
            if (!string.IsNullOrEmpty(packageSources))
                packageSourcesArgument = @"-Source " + packageSources + "";

            var outputDirectoryArgument = "";
            if (!string.IsNullOrEmpty(workingDirectory))
                outputDirectoryArgument = @"-OutputDirectory " + workingDirectory + "";

            if (!string.IsNullOrEmpty(nugetPath))
                nugetPath += @"\";

            var installedPackages = new List<string>();

            var strOutput = _commandRunner.RunCommand(nugetPath + @"nuget", string.Format(@"install {0} {1} {2}", package, outputDirectoryArgument, packageSourcesArgument), workingDirectory);

            var matches = Regex.Matches(strOutput, @"Successfully installed '([^']*)'\.", RegexOptions.CultureInvariant);
            foreach (Match m in matches)
                if (m.Success)
                {
                    installedPackages.Add(m.Groups[1].Value);
                }

            matches = Regex.Matches(strOutput, @"'([^']*)' already installed\.", RegexOptions.CultureInvariant);
            foreach (Match m in matches)
                if (m.Success)
                {
                    installedPackages.Add(m.Groups[1].Value);
                }

            if (!installedPackages.Any())
                throw new ConsoleException(2);

            return installedPackages.Select(installedPackage => installedPackage.Replace(' ', '.')).ToList();
        }
    }
}