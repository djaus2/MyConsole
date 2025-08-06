using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sportronics.ConfigurationManager
{
    /// <summary>
    /// Core class for loading, processing, and saving configuration settings
    /// </summary>
    /// <typeparam name="T">Type of settings class, must inherit from AppSettingsBase</typeparam>
    public class ConfigurationProcessor<T> where T : AppSettingsBase, new()
    {
        private readonly string _jsonFilePath;
        private readonly CommandLineParser _commandLineParser;
        private T _settings;
        private readonly Dictionary<string, (string LongName, string ShortName)> _optionsMap;
        /// <summary>
        /// Gets the current settings instance
        /// </summary>
        public T? Settings => _settings;

        /// <summary>
        /// Initializes a new instance of the ConfigurationProcessor class
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON configuration file</param>
        /// <param name="optionsMap">Dictionary mapping property names to command line option names</param>
        /// <param name="defaultSettings">Optional default settings to use if not specified elsewhere</param>
        public ConfigurationProcessor(
            string jsonFilePath,
            Dictionary<string, (string LongName, string ShortName)> optionsMap,
            T? defaultSettings = null)
        {
            _jsonFilePath = jsonFilePath;
            _optionsMap = optionsMap;
            _settings = defaultSettings?? new();
            _commandLineParser = new CommandLineParser();

            // Register command line options
            _commandLineParser.RegisterOptionsFromClass(_settings, _optionsMap);
        }

        /// <summary>
        /// Processes configuration from all sources in order of precedence:
        /// 1. Command line arguments (highest priority)
        /// 2. In-app settings (if provided and not ignoring stored settings)
        /// 3. JSON configuration file (if not ignoring stored settings)
        /// 4. Default values (lowest priority)
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="saveSettings">Whether to save the final settings back to the JSON file</param>
        /// <returns>The processed settings</returns>
        public T? ProcessConfiguration(string[] args, bool saveSettings = true)
        {
            bool hasCommandLineArgs = false;
            T? originalSettings = null;
            
            // Store original settings if they exist
            if (_settings != null)
            {
                originalSettings = _settings;
            }


            // Apply command line arguments first to check for flags
            if (args != null && args.Length > 0)
            {
                ApplyCommandLineArgs(args);
                hasCommandLineArgs = true;

                string[] helpArgs = { "--help", "-h", "-i", "--ignore" };
                if (helpArgs.Contains(args[0], StringComparer.OrdinalIgnoreCase))
                {
                    hasCommandLineArgs = false;
                }
                // If help flag is set, display help and return current settings
                if (_commandLineParser.HelpRequested)
                {
                    DisplayHelp();
                    return null;
                }

                // If reset flag is set, reset to default settings
                if (_commandLineParser.ResetRequested)
                {
                    Console.WriteLine("Resetting to default settings...");
                    _settings = new T();

                    if (saveSettings)
                    {
                        SaveSettingsToJson();
                        Console.WriteLine("Default settings saved to configuration file.");
                    }

                    return _settings;
                }
                
            }
            
            // Save command line settings to restore later
            T? commandLineSettings = null;
            if (hasCommandLineArgs)
            {
                commandLineSettings = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(_settings));
            }
            
            // Determine settings source based on flags and available settings
            if (_commandLineParser.IgnoreStoredSettings)
            {
                // Ignore stored settings flag is set, use in-app settings if available
                if (originalSettings != null)
                {
                    _settings = originalSettings;
                    Console.WriteLine("Ignoring stored settings, using in-app settings...");
                }
                else
                {
                    // No in-app settings available, use defaults
                    _settings = new T();
                    Console.WriteLine("Ignoring stored settings, using default settings...");
                }
            }
            else if (!hasCommandLineArgs)
            {
                // No command line args, try to load from JSON file
                try
                {
                    LoadSettingsFromJson();
                }
                catch (Exception)
                {
                    // If loading from JSON fails, fall back to in-app settings or defaults
                    if (originalSettings != null)
                    {
                        _settings = originalSettings;
                        Console.WriteLine("No stored settings found, using in-app settings...");
                    }
                    else
                    {
                        _settings = new T();
                        Console.WriteLine("No stored settings found, using default settings...");
                    }
                }
            }

            if (_settings == null)
            {
                _settings = new T();
            }
            
            // Restore command line settings to ensure they have highest precedence
            if (commandLineSettings != null)
            {
                // Copy non-default properties from command line settings to final settings
                var properties = typeof(T).GetProperties()
                    .Where(p => p.CanRead && p.CanWrite);
                
                foreach (var prop in properties)
                {
                    var defaultValue = prop.GetValue(new T());
                    var commandLineValue = prop.GetValue(commandLineSettings);
                    
                    // Only override if command line value is different from default
                    if (!Equals(commandLineValue, defaultValue))
                    {
                        prop.SetValue(_settings, commandLineValue);
                    }
                }
                
                Console.WriteLine("Applied command line settings with highest precedence.");
            }
            
            // Save settings if requested
            if (saveSettings && !_commandLineParser.IgnoreStoredSettings)
            {
                SaveSettingsToJson();
            }

            return _settings;
        }

        /// <summary>
        /// Loads settings from the JSON configuration file
        /// </summary>
        public void LoadSettingsFromJson()
        {
            try
            {
                // Build configuration from JSON file
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(_jsonFilePath, optional: false, reloadOnChange: true);

                var jsonConfig = configBuilder.Build();

                // Apply settings from JSON file
                jsonConfig.GetSection(_settings.SectionName).Bind(_settings);
                Console.WriteLine($"Using stored settings.");
            }
            catch (Exception ex)
            {
                _settings = new();
                // Log error or handle exception
                Console.WriteLine($"No in-app, stored or CMD line settings, using default.");
            }
        }

        /// <summary>
        /// Applies command line arguments to the settings
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public void ApplyCommandLineArgs(string[] args)
        {
            try
            {
                // Process command line arguments
                string[] configArgs = _commandLineParser.ProcessArgs(args);

                if (configArgs.Length > 0)
                {
                    var commandLineConfig = new ConfigurationBuilder()
                        .AddCommandLine(configArgs)
                        .Build();

                    commandLineConfig.GetSection(_settings.SectionName).Bind(_settings);
                    Console.WriteLine($"Using CMD line settings.");
                }
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                Console.WriteLine($"Error applying command line arguments: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current settings to the JSON configuration file
        /// </summary>
        public void SaveSettingsToJson()
        {
            try
            {
                // Read the existing JSON file
                string json = File.ReadAllText(_jsonFilePath);

                // Deserialize to a dictionary
                var jsonDocument = JsonDocument.Parse(json);
                var rootElement = jsonDocument.RootElement;

                // Create a new JSON object with updated settings
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();

                    // Copy all properties from the original JSON
                    foreach (var property in rootElement.EnumerateObject())
                    {
                        if (property.Name == _settings.SectionName)
                        {
                            // Replace section with our updated values
                            writer.WritePropertyName(_settings.SectionName);
                            JsonSerializer.Serialize(writer, _settings, _settings.GetType());
                        }
                        else
                        {
                            // Copy other properties as is
                            property.WriteTo(writer);
                        }
                    }

                    writer.WriteEndObject();
                }

                // Convert to string and save
                var updatedJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(_jsonFilePath, updatedJson);
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                Console.WriteLine($"Error saving settings to JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays help information for command line options
        /// </summary>
        public void DisplayHelp()
        {
            Console.WriteLine("\nSportronics.ConfigurationManager Help");
            Console.WriteLine("=====================================");
            Console.WriteLine("Command Line Options:");
            Console.WriteLine("  -h, --help     : Display this help information");
            Console.WriteLine("  -r, --reset    : Reset all settings to default values");
            Console.WriteLine("  -i, --ignore   : Ignore stored settings and use in-app or default settings");
            Console.WriteLine("\nRegistered Configuration Options:");
            
            // Display registered options
            foreach (var option in _optionsMap)
            {
                string propertyName = option.Key;
                var (longName, shortName) = option.Value;
                
                Console.WriteLine($"  -{shortName}, --{longName} <value> : Set {propertyName}");
            }
            
            Console.WriteLine("\nConfiguration Precedence:");
            Console.WriteLine("1. Command line arguments (highest priority)");
            Console.WriteLine("2. In-app settings (if provided and not ignoring stored settings)");
            Console.WriteLine("3. JSON configuration file (if not ignoring stored settings)");
            Console.WriteLine("4. Default values (lowest priority)");
            Console.WriteLine();
        }
    }
}
