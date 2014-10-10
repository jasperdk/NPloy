using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPloy.Support
{
    public interface INuGetRunner
    {
        IList<string> RunNuGetInstall(string package, string version, string packageSources, string nugetPath, string workingDirectory, bool includePrereleases = false);

        Dictionary<string, string> RunNuGetList(string packageSources, string nugetPath, string workingDirectory,
                                                bool includePrereleases = false);
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

        public IList<string> RunNuGetInstall(string package, string version, string packageSources, string nugetPath,
                                             string workingDirectory, bool includePrereleases = false)
        {
            var packageSourcesArgument = "";
            if (!string.IsNullOrEmpty(packageSources))
                packageSourcesArgument = @"-Source " + packageSources + "";

            var outputDirectoryArgument = "";
            if (!string.IsNullOrEmpty(workingDirectory))
                outputDirectoryArgument = @"-OutputDirectory " + workingDirectory + "";

            var versionArgument = "";
            if (!string.IsNullOrEmpty(version))
                versionArgument = @"-Version " + version + "";

            var nugetFile = GetNugetFile(nugetPath);

            var prereleaseFlag = "";
            if (includePrereleases)
                prereleaseFlag = "-Prerelease";

            var installedPackages = new List<string>();
            var script = string.Format(@"install {0} {1} {2} {3} {4}", package, outputDirectoryArgument,
                                       packageSourcesArgument,
                                       versionArgument, prereleaseFlag);
            var strOutput = _commandRunner.RunCommand(nugetFile, script, workingDirectory);

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

        private static string GetNugetFile(string nugetPath)
        {
            var nugetFile = @"nuget.exe";
            if (!string.IsNullOrEmpty(nugetPath))
                nugetFile = Path.Combine(nugetPath, nugetFile);
            return nugetFile;
        }

        public Dictionary<string, string> RunNuGetList(string packageSources, string nugetPath, string workingDirectory,
                                                       bool includePrereleases = false)
        {
            var nugetFile = GetNugetFile(nugetPath);

            var packageSourcesArgument = "";
            if (!string.IsNullOrEmpty(packageSources))
                packageSourcesArgument = @"-Source " + packageSources + "";

            var prereleaseFlag = "";
            if (includePrereleases)
                prereleaseFlag = "-Prerelease";

            var script = string.Format(@"list {0} {1}", packageSourcesArgument, prereleaseFlag);

            var strOutput = _commandRunner.RunCommand(nugetFile, script, workingDirectory);

            var packagesList = new List<string>(
                strOutput.Split(new[] {"\r\n"},
                                StringSplitOptions.RemoveEmptyEntries));

            return packagesList.Select(packageLine => packageLine.Split(' '))
                               .ToDictionary(packageInfo => packageInfo[0], packageInfo => packageInfo[1]);

        }
    }
}