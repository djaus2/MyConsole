using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    /// <summary>
    /// Core class for loading, processing, and saving configuration settings
    /// </summary>
    /// <typeparam name="T">Type of settings class, must inherit from AppSettingsBase</typeparam>
    public class ConfigurationProcessor<T> where T : AppSettingsBase, new()
    {
        private readonly string _jsonFilePath;
        private readonly CommandLineParser _commandLineParser;
        private readonly T _settings;
        private readonly Dictionary<string, (string LongName, string ShortName)> _optionsMap;

        /// <summary>
        /// Gets the current settings instance
        /// </summary>
        public T Settings => _settings;

        /// <summary>
        /// Initializes a new instance of the ConfigurationProcessor class
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON configuration file</param>
        /// <param name="optionsMap">Dictionary mapping property names to command line option names</param>
        /// <param name="defaultSettings">Optional default settings to use if not specified elsewhere</param>
        public ConfigurationProcessor(
            string jsonFilePath,
            Dictionary<string, (string LongName, string ShortName)> optionsMap,
            T defaultSettings = null)
        {
            _jsonFilePath = jsonFilePath;
            _optionsMap = optionsMap;
            _settings = defaultSettings ?? new T();
            _commandLineParser = new CommandLineParser();
            
            // Register command line options
            _commandLineParser.RegisterOptionsFromClass(_settings, _optionsMap);
        }

        /// <summary>
        /// Processes configuration from all sources in order of precedence:
        /// 1. Command line arguments (highest priority)
        /// 2. JSON configuration file
        /// 3. Default values (lowest priority)
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="saveSettings">Whether to save the final settings back to the JSON file</param>
        /// <returns>The processed settings</returns>
        public T ProcessConfiguration(string[] args, bool saveSettings = true)
        {
            // Load settings from JSON file
            LoadSettingsFromJson();
            
            // Apply command line arguments
            if (args != null && args.Length > 0)
            {
                ApplyCommandLineArgs(args);
            }
            
            // Save settings if requested
            if (saveSettings)
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
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                Console.WriteLine($"Error loading settings from JSON: {ex.Message}");
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
    }
}
