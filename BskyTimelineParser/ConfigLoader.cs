using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EbonCorvin
{
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
                foreach(var item in configDict)
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

        // A really simple config loader, it read a plain text file line by line, than splits each line into key and value by "="
        public ConfigLoader(String filePath)
        {
            path = filePath;
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open));
            String config = reader.ReadToEnd();
            reader.Close();
            configDict = new Dictionary<String, String>();
            int curPos = 0;
            while (curPos < config.Length)
            {
                int divPos = config.IndexOf("=", curPos);
                String key = config.Substring(curPos, divPos - curPos);
                int configEnd = config.IndexOf(Environment.NewLine, divPos + 1);
                if (configEnd == -1)
                    configEnd = config.Length;
                String value = config.Substring(divPos + 1, configEnd - divPos - 1);
                curPos = configEnd + 2;
                configDict.Add(key, value);
            }            
        }
    }
}
