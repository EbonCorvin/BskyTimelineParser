using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace EbonCorvin.Utils
{
    /// <summary>
    /// A simple configuration loader. It stores the configuration in a key-value format line by line.
    /// </summary>
    public class ConfigLoader
    {
        private string path = "";
        private Dictionary<string, string> configDict = null;

        public string this[string key]
        {
            get
            {
                bool hasValue = configDict.TryGetValue(key, out var value);
                // if (!hasValue)
                // throw new IndexOutOfRangeException(key);
                return value;
            }

            set
            {
                configDict[key] = value;
                StringBuilder sb = new StringBuilder();
                foreach (var item in configDict)
                {
                    sb.Append(item.Key);
                    sb.Append('=');
                    sb.Append(item.Value);
                    sb.AppendLine();
                }
                File.WriteAllText(path, sb.ToString());
            }
        }
        public bool TryGet(string key, out string value)
        {
            return configDict.TryGetValue(key, out value);
        }
        
        /// <summary>
        /// Indicate whether the configuration file exsits or not.
        /// It allows the caller to create a new config file with some default settings
        /// </summary>
        public bool IsFileExists
        {
            private set;
            get;
        } = true;


        // A really simple config loader, it read a plain text file line by line, than splits each line into key and value by "="
        public ConfigLoader(string filePath)
        {
            configDict = new Dictionary<string, string>();
            path = filePath;
            if (!File.Exists(path))
            {
                IsFileExists = false;
                return;
            }
            StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open));
            string config = reader.ReadToEnd();
            reader.Close();
            int curPos = 0;
            while (curPos < config.Length)
            {
                int divPos = config.IndexOf("=", curPos);
                string key = config.Substring(curPos, divPos - curPos);
                int configEnd = config.IndexOf(Environment.NewLine, divPos + 1);
                if (configEnd == -1)
                    configEnd = config.Length;
                string value = config.Substring(divPos + 1, configEnd - divPos - 1);
                curPos = configEnd + 2;
                configDict.Add(key, value);
            }
        }
    }
}
