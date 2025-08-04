using ConfigurationManager;
using System;
using System.Collections.Generic;
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
            var defaultSettings = new AppSettings
            {
                Folder = "C:\\DefaultFolder",
                Port = 8080
            };
            
            Console.WriteLine("Default settings: " + defaultSettings);
            
            // Define command line options mapping
            var optionsMap = new Dictionary<string, (string LongName, string ShortName)>
            {
                { "Folder", ("folder", "f") },
                { "Port", ("port", "p") }
            };
            
            // Create configuration processor
            var configProcessor = new ConfigurationProcessor<AppSettings>(
                "appsettings.json",
                optionsMap,
                defaultSettings);
            
            // Process configuration from all sources
            if (args.Length > 0)
            {
                Console.WriteLine("Command line arguments detected: " + string.Join(" ", args));
            }
            
            var appSettings = configProcessor.ProcessConfiguration(args);
            
            // Final settings after all sources have been applied
            Console.WriteLine("\nFinal Application Settings:");
            Console.WriteLine(appSettings);
            
            Console.WriteLine("\nSettings saved successfully.");

            // Your application logic here
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
