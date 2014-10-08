using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace NPloy.Support
{
    public interface INPloyConfiguration
    {
        IList<string> GetFiles(string s);
        Dictionary<string, string> GetProperties(string package, string environment, string configurationDirectory);
        void SubstituteValues(Dictionary<string, string> configProperties);
        bool FileExists(string filePath);
        bool HasInstalledPackages(string workingDirectory);
        IEnumerable<string> GetInstalledPackges(string workingDirectory);
        void PackagesHasBeenUninstalled(string workingDirectory);
        RoleConfig GetRoleConfig(string roleFile);
        XmlDocument GetNodeXml(string nodeFile);
    }

    public class PackageConfig
    {
        public string Id
        {
            get;
            set;
        }

        public string Version { get; set; }

        public string FullName
        {
            get { return Id + "." + Version; }
        }
    }

    public class RoleConfig
    {
        public RoleConfig()
        {
            Packages = new List<PackageConfig>();
        }

        public string SubFolder
        {
            get;
            set;
        }

        public IList<PackageConfig> Packages { get; set; }
    }

    public class NPloyConfiguration : INPloyConfiguration
    {
        private const string Pattern = @"\$\(([^\)]*)\)";
        public IList<string> GetFiles(string path)
        {
            if (!Directory.Exists(path))
                return new List<string>();
            return Directory.GetFiles(path);
        }

        public Dictionary<string, string> GetProperties(string package, string environment, string configurationDirectory)
        {
            if (!string.IsNullOrEmpty(configurationDirectory))
                configurationDirectory += @"\";
            Console.WriteLine("Loading properties for environment: " + environment);
            var result = new Dictionary<string, string>();
            GetPropertiesFromFile(configurationDirectory + @"environments\default\env.prop", result);
            GetPropertiesFromFile(configurationDirectory + @"environments\default\" + package + ".prop", result);
            if (environment.ToLower() != "default")
            {
                GetPropertiesFromFile(configurationDirectory + @"environments\" + environment + @"\env.prop", result);
                GetPropertiesFromFile(
                    configurationDirectory + @"environments\" + environment + @"\" + package + ".prop", result);
            }

            return result;
        }

        public void SubstituteValues(Dictionary<string, string> configProperties)
        {
            var substitutionCandidates = configProperties.Where(x => Regex.IsMatch(x.Value, Pattern)).ToArray();
            foreach (var substitutionCandidate in substitutionCandidates)
            {
                Substitute(configProperties, substitutionCandidate);
            }
        }

        private static void Substitute(Dictionary<string, string> configProperties, KeyValuePair<string, string> substitutionCandidate)
        {
            var match = Regex.Match(substitutionCandidate.Value, Pattern);
            if (!match.Success) return;

            var substitutionValue = "";
            var substitutionKey = match.Groups[1].Value.Replace(".", "");
            if (configProperties.ContainsKey(substitutionKey) && substitutionKey != substitutionCandidate.Key)
                substitutionValue = configProperties[substitutionKey];
            configProperties[substitutionCandidate.Key] = Regex.Replace(substitutionCandidate.Value, @"\$\(" + match.Groups[1].Value + @"\)", substitutionValue);

            if (Regex.IsMatch(configProperties[substitutionCandidate.Key], Pattern))
                Substitute(configProperties, configProperties.Single(x => x.Key == substitutionCandidate.Key));
        }

        private static void GetPropertiesFromFile(string file, Dictionary<string, string> result)
        {
            if (File.Exists(file))
            {
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var items = line.Split('=');
                    var key = items[0].Replace(".", "");
                    var value = line.Remove(0, items[0].Length + 1);
                    result[key] = value;
                }
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public bool HasInstalledPackages(string workingDirectory)
        {
            return File.Exists(workingDirectory + @"\packages.config");
        }

        public IEnumerable<string> GetInstalledPackges(string workingDirectory)
        {
            return File.ReadAllLines(workingDirectory + @"\packages.config");
        }

        public void PackagesHasBeenUninstalled(string workingDirectory)
        {
            File.Delete(workingDirectory + @"\packages.config");
        }

        public RoleConfig GetRoleConfig(string roleFile)
        {
            var roleConfig = new RoleConfig();

            var doc = new XmlDocument();
            doc.Load(roleFile);
            var rootElement = doc.DocumentElement;
            roleConfig.SubFolder = rootElement.Attributes["subFolder"] != null ? rootElement.Attributes["subFolder"].Value : "";

            var docPackages = doc.GetElementsByTagName("package");
            foreach (XmlNode docPackage in docPackages)
            {
                roleConfig.Packages.Add(new PackageConfig
                {
                    Id = docPackage.Attributes["id"].Value,
                    Version =
                        docPackage.Attributes["version"] != null ? docPackage.Attributes["version"].Value : null
                });
            }
            return roleConfig;
        }

        public XmlDocument GetNodeXml(string nodeFile)
        {
            var doc = new XmlDocument();
            doc.Load(nodeFile);
            return doc;
        }
    }
}
