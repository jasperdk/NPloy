﻿using System.Collections.Generic;
using System.IO;

namespace NPloy.Support
{
    public interface INPloyConfiguration
    {
        IList<string> GetFiles(string s);
        Dictionary<string,string> GetProperties(string environment);
    }

    public class NPloyConfiguration:INPloyConfiguration
    {
        public IList<string> GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public Dictionary<string,string> GetProperties(string environment)
        {
            var result = new Dictionary<string, string>();
            var lines = File.ReadAllLines(@".nploy\environments\" + environment + @"\env.prop");
            foreach (var line in lines)
            {
                var items = line.Split('=');
                var key = items[0].Replace(".", "");
                var value = line.Remove(0, items[0].Length + 1);
                result.Add(key, value);
            }
            return result;
        }
    }
}