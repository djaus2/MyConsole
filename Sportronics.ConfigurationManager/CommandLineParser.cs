using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sportronics.ConfigurationManager
{
    /// <summary>
    /// Handles parsing of command line arguments in both long and short forms
    /// </summary>
    public class CommandLineParser
    {
        private readonly Dictionary<string, CommandLineOption> _options = new Dictionary<string, CommandLineOption>();
        
        /// <summary>
        /// Flag indicating if the reset option was specified in the command line arguments
        /// </summary>
        public bool ResetRequested { get; private set; }
        
        /// <summary>
        /// Flag indicating if stored settings should be ignored (use in-app settings instead)
        /// </summary>
        public bool IgnoreStoredSettings { get; private set; }
        
        /// <summary>
        /// Flag indicating if help information was requested
        /// </summary>
        public bool HelpRequested { get; private set; }
        
        /// <summary>
        /// Adds a command line option with both long and short forms
        /// </summary>
        /// <param name="longName">Long form name (e.g., "folder")</param>
        /// <param name="shortName">Short form name (e.g., "f")</param>
        /// <param name="sectionName">Configuration section name</param>
        /// <param name="propertyName">Property name within the section</param>
        public void AddOption(string longName, string shortName, string sectionName, string propertyName)
        {
            var option = new CommandLineOption
            {
                LongName = longName,
                ShortName = shortName,
                SectionName = sectionName,
                PropertyName = propertyName
            };
            
            _options[longName] = option;
            _options[shortName] = option;
        }
        
        /// <summary>
        /// Processes command line arguments into the format expected by ConfigurationBuilder
        /// </summary>
        /// <param name="args">Original command line arguments</param>
        /// <returns>Processed arguments in ConfigurationBuilder format</returns>
        public string[] ProcessArgs(string[] args)
        {
            List<string> configArgs = new List<string>();
            ResetRequested = false;
            IgnoreStoredSettings = false;
            HelpRequested = false;
            
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                bool processed = false;
                
                // Check for help flag
                if (arg == "-h" || arg == "--help")
                {
                    HelpRequested = true;
                    processed = true;
                    continue;
                }
                
                // Check for reset flag
                if (arg == "-r" || arg == "--reset")
                {
                    ResetRequested = true;
                    processed = true;
                    continue;
                }
                
                // Check for ignore stored settings flag
                if (arg == "-i" || arg == "--ignore")
                {
                    IgnoreStoredSettings = true;
                    processed = true;
                    continue;
                }
                
                // Handle arguments with equals sign (--option=value or -o=value)
                if (arg.Contains("="))
                {
                    int equalsIndex = arg.IndexOf('=');
                    string optionName = arg.Substring(0, equalsIndex).TrimStart('-');
                    string value = arg.Substring(equalsIndex + 1);
                    
                    if (_options.TryGetValue(optionName, out CommandLineOption option))
                    {
                        configArgs.Add($"--{option.SectionName}:{option.PropertyName}={value}");
                        processed = true;
                    }
                }
                // Handle space-separated arguments (--option value or -o value)
                else if (arg.StartsWith("-"))
                {
                    string optionName = arg.TrimStart('-');
                    
                    if (_options.TryGetValue(optionName, out CommandLineOption option) && i + 1 < args.Length)
                    {
                        configArgs.Add($"--{option.SectionName}:{option.PropertyName}={args[++i]}");
                        processed = true;
                    }
                }
                
                // Pass through any arguments in the original format if not processed
                if (!processed && arg.StartsWith("--") && arg.Contains(":"))
                {
                    configArgs.Add(arg);
                }
            }
            
            return configArgs.ToArray();
        }
        
        /// <summary>
        /// Automatically registers command line options based on properties of a settings class
        /// </summary>
        /// <typeparam name="T">Type of settings class</typeparam>
        /// <param name="instance">Instance of settings class</param>
        /// <param name="optionsMap">Dictionary mapping property names to option names</param>
        public void RegisterOptionsFromClass<T>(T instance, Dictionary<string, (string LongName, string ShortName)> optionsMap) where T : AppSettingsBase
        {
            if (instance == null)
                return;
            string sectionName = instance.SectionName;
            Type type = typeof(T);
            
            foreach (var property in type.GetProperties())
            {
                if (optionsMap.TryGetValue(property.Name, out var option))
                {
                    AddOption(option.LongName, option.ShortName, sectionName, property.Name);
                }
            }
        }
        
        private class CommandLineOption
        {
            public string LongName { get; set; }
            public string ShortName { get; set; }
            public string SectionName { get; set; }
            public string PropertyName { get; set; }
        }
    }
}
