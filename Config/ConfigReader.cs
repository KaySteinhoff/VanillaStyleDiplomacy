using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem.Actions;
using VanillaStyleDiplomacy.Extensions;
using VanillaStyleDiplomacy.Helpers;
using VanillaStyleDiplomacy.Logging;

namespace VanillaStyleDiplomacy.Config
{
    public class ConfigReader
    {
        private static ConfigReader instance = null;
        private Dictionary<string, object> configOptions = new Dictionary<string, object>();
        private Dictionary<string, Type> configTypeDefs = new Dictionary<string, Type>();
        Regex configOptionsRegex = new Regex(@"^(?<TYPE>\w+)*(?<OPTION_NAME>\s+\w+)*\s*=*\s*(?<VALUE>[^#]+)*(?<OPTIONAL_COMMENT>\s*[#]+.*$)*");

        public static ConfigReader Instance
        {
            get
            {
                if (instance == null)
                    instance = new ConfigReader();
                return instance;
            }
        }

        public object this[string key]
        {
            get
            {
                if (!configOptions.TryGetValue(key, out object result))
                    return null;
                return result;
            }
        }

        private ConfigReader()
        {
            configTypeDefs.Add("int", typeof(int));
            configTypeDefs.Add("double", typeof(int));
            configTypeDefs.Add("string", typeof(int));
            configTypeDefs.Add("bool", typeof(bool));

            StreamReader reader = null;
            try
            {
                if (!File.Exists($"{ConstantsHelper.ModuleFolder}/ModuleData/config.dat"))
                {
                    LoggingManager.Instance.LogMessage("No config file found! Generating default config...");
                    GenerateDefaultConfig();
                    LoggingManager.Instance.LogMessage("Successfully generated default config!");
                }
                reader = new StreamReader($"{ConstantsHelper.ModuleFolder}/ModuleData/config.dat", Encoding.UTF8);
            }
            catch (Exception e)
            {
                LoggingManager.Instance.LogMessage($"Failed to read config file because it was moved or deleted!");
                LoggingManager.Instance.LogException(e);
                return;
            }

            try
            {
                ParseConfigOptions(reader);
            }
            catch (Exception e)
            {
                LoggingManager.Instance.LogException(e);
            }

            reader.Close();
        }

        private void ParseConfigOptions(StreamReader reader)
        {
            LoggingManager.Instance.LogMessage("Parsing config options...");
            string line;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                Match regexMatch = configOptionsRegex.Match(line);

                bool anyOptionValueSet = regexMatch.Groups["TYPE"].Success || regexMatch.Groups["OPTION_NAME"].Success || regexMatch.Groups["VALUE"].Success;
                // Check if any value wasn't defined, then check if any value was
                // If both statements return true we know atleast one value is missing
                // Otherwise it's empty or a comment
                if (!regexMatch.Groups["TYPE"].Success ||
                    !regexMatch.Groups["OPTION_NAME"].Success ||
                    !regexMatch.Groups["VALUE"].Success)
                {
                    if (anyOptionValueSet)
                        LoggingManager.Instance.LogException(new InvalidDataException($"Insuffitient data on line {lineNumber}!"));
                    continue;
                }

                Type optionType = null;
                if (!configTypeDefs.TryGetValue(regexMatch.Groups["TYPE"].Value, out optionType))
                {
                    LoggingManager.Instance.LogException(new InvalidDataException($"Failed to find type def for value '{regexMatch.Groups["TYPE"].Value}'!"));
                    continue;
                }
                string optionName = regexMatch.Groups["OPTION_NAME"].Value.Trim();
                if (configOptions.ContainsKey(optionName))
                {
                    LoggingManager.Instance.LogException(new InvalidDataException($"Config option with the name {optionName} already exists!"));
                    continue;
                }

                if (!regexMatch.Groups["VALUE"].Value.TryCastOnRunTime(optionType, out object value))
                    LoggingManager.Instance.LogException(new InvalidCastException($"Failed to cast config option {optionName} on runtime!"));
                else
                    configOptions.Add(optionName, value);
            }
            LoggingManager.Instance.LogMessage("Successfuly parsed config options!");
        }

        private void GenerateDefaultConfig()
        {
            try
            {
                StreamWriter writer = new StreamWriter($"{ConstantsHelper.ModuleFolder}/ModuleData/config.dat", false, Encoding.UTF8);
                writer.WriteLine(ConstantsHelper.DefaultConfigFile);
                writer.Close();
            }
            catch (Exception e)
            {
                LoggingManager.Instance.LogMessage($"Failed to generate default config file!");
                LoggingManager.Instance.LogException(e);
            }
        }

        public bool TryGetConfigOption(string key, out object result)
        {
            return configOptions.TryGetValue(key, out result);
        }
    }
}