using System.Collections.Generic;
using System.IO;

namespace NPloy.Support
{
    public interface INPloyConfiguration
    {
        IList<string> GetFiles(string s);
        Dictionary<string, string> GetProperties(string package, string environment);
        bool FileExists(string filePath);
        bool HasInstalledPackages(string workingDirectory);
        IEnumerable<string> GetInstalledPackges(string workingDirectory);
        void PackagesHasBeenUninstalled(string workingDirectory);
    }

    public class NPloyConfiguration:INPloyConfiguration
    {
        public IList<string> GetFiles(string path)
        {
            if(!Directory.Exists(path))
                return new List<string>();
            return Directory.GetFiles(path);
        }

        public Dictionary<string, string> GetProperties(string package, string environment)
        {
            var result = new Dictionary<string, string>();
            GetPropertiesFromFile(@".nploy\environments\default.prop", result);
            GetPropertiesFromFile(@".nploy\environments\" + environment + @"\env.prop", result);
            GetPropertiesFromFile(@".nploy\environments\" + environment + @"\" + package + ".prop", result);
            return result;
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
    }
}
