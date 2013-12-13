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
        bool FileExists(string filePath);
        bool HasInstalledPackages(string workingDirectory);
        IEnumerable<string> GetInstalledPackges(string workingDirectory);
        void PackagesHasBeenUninstalled(string workingDirectory);
        RoleConfig GetRoleConfig(string roleFile);
    }

    public class PackageConfig
    {
        public string Id
        {
            get;
            set;
        }

        public string Version { get; set; }
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

            SubstituteValues(result);

            return result;
        }

        private static void SubstituteValues(Dictionary<string, string> result)
        {
            var substitutionCandidates = result.Where(x => Regex.IsMatch(x.Value, Pattern)).ToArray();
            for (var i = 0; i < substitutionCandidates.Length; i++)
            {
                var keyValue = substitutionCandidates[i];
                Substitute(result, keyValue);
            }
        }

        private static void Substitute(Dictionary<string, string> result, KeyValuePair<string, string> keyValue)
        {
            var match = Regex.Match(keyValue.Value, Pattern);
            if (match.Success)
            {
                var substitutionValue = "";
                var substitutionKey = match.Groups[1].Value;
                if (result.ContainsKey(substitutionKey) && substitutionKey != keyValue.Key)
                    substitutionValue = result[substitutionKey];
                result[keyValue.Key] = Regex.Replace(keyValue.Value, @"\$\(" + substitutionKey + @"\)", substitutionValue);

                if (Regex.IsMatch(result[keyValue.Key], Pattern))
                    Substitute(result, result.Single(x => x.Key == keyValue.Key));
            }
           
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
    }
}
