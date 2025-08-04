using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting application...");
            
            // Default settings (lowest priority)
            var appSettings = new AppSettings
            {
                Folder = "C:\\DefaultFolder",
                Port = 8080
            };
            
            Console.WriteLine("Default settings: " + appSettings);
            
            // Build configuration from appsettings.json
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                
            var jsonConfig = configBuilder.Build();
            
            // Apply settings from JSON file (medium priority)
            jsonConfig.GetSection("AppSettings").Bind(appSettings);
            Console.WriteLine("Settings after reading from appsettings.json: " + appSettings);
            
            // Process simple command line arguments (highest priority)
            if (args.Length > 0)
            {
                Console.WriteLine("Command line arguments detected: " + string.Join(" ", args));
                
                // Convert simple arguments to the format expected by ConfigurationBuilder
                string[] configArgs = ProcessSimpleArgs(args);
                
                if (configArgs.Length > 0)
                {
                    var commandLineConfig = new ConfigurationBuilder()
                        .AddCommandLine(configArgs)
                        .Build();
                        
                    commandLineConfig.GetSection("AppSettings").Bind(appSettings);
                    Console.WriteLine("Settings after applying command line arguments: " + appSettings);
                }
            }
            
            // Final settings after all sources have been applied
            Console.WriteLine("\nFinal Application Settings:");
            Console.WriteLine(appSettings);

            // Save the used values back to appsettings.json
            SaveSettings(appSettings);

            // Your application logic here
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // Process simple command line arguments into the format expected by ConfigurationBuilder
        static string[] ProcessSimpleArgs(string[] args)
        {
            List<string> configArgs = new List<string>();
            
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                
                // Handle --folder or -f
                if (arg.Equals("--folder", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("-f", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        configArgs.Add($"--AppSettings:Folder={args[++i]}");
                    }
                }
                // Handle --port or -p
                else if (arg.Equals("--port", StringComparison.OrdinalIgnoreCase) || 
                         arg.Equals("-p", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        configArgs.Add($"--AppSettings:Port={args[++i]}");
                    }
                }
                // Handle direct assignment with equals sign (--folder=path or -f=path)
                else if (arg.StartsWith("--folder=", StringComparison.OrdinalIgnoreCase) || 
                         arg.StartsWith("-f=", StringComparison.OrdinalIgnoreCase))
                {
                    string value = arg.Substring(arg.IndexOf('=') + 1);
                    configArgs.Add($"--AppSettings:Folder={value}");
                }
                else if (arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase) || 
                         arg.StartsWith("-p=", StringComparison.OrdinalIgnoreCase))
                {
                    string value = arg.Substring(arg.IndexOf('=') + 1);
                    configArgs.Add($"--AppSettings:Port={value}");
                }
                // Pass through any arguments in the original format
                else if (arg.StartsWith("--AppSettings:"))
                {
                    configArgs.Add(arg);
                }
            }
            
            return configArgs.ToArray();
        }

        static void SaveSettings(AppSettings settings)
        {
            try
            {
                // Read the existing JSON file
                string json = File.ReadAllText("appsettings.json");
                
                // Deserialize to a dictionary
                var jsonDocument = JsonDocument.Parse(json);
                var rootElement = jsonDocument.RootElement;
                
                // Create a new JSON object with updated settings
                var options = new JsonSerializerOptions { WriteIndented = true };
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();
                    
                    // Copy all properties from the original JSON
                    foreach (var property in rootElement.EnumerateObject())
                    {
                        if (property.Name == "AppSettings")
                        {
                            // Replace AppSettings with our updated values
                            writer.WritePropertyName("AppSettings");
                            writer.WriteStartObject();
                            writer.WriteString("Folder", settings.Folder);
                            writer.WriteNumber("Port", settings.Port);
                            writer.WriteEndObject();
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
                File.WriteAllText("appsettings.json", updatedJson);
                
                Console.WriteLine("\nSettings saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError saving settings: {ex.Message}");
            }
        }
    }
}
