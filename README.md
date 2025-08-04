# MyConsole Application

A C# console application that demonstrates configuration management with the following features:

- Configuration properties:
  - `Folder` (string)
  - `Port` (int)
- Configuration sources (in order of precedence):
  1. Command line arguments
  2. appsettings.json file
- Automatically saves the used configuration values back to appsettings.json

> Configuration code is in a companion project

## Projecvts
- MyConsole: The console app
- ConfigurationManager: The backend lib

## Building and Running

### Prerequisites
- .NET 9.0 SDK or later

### Build
```
dotnet build
```

### Run
```
dotnet run
```

### Run with command line arguments
You can now use simplified command line arguments:

```
# Using long form arguments
dotnet run -- --folder "C:\CustomFolder" --port 9090

# Using short form arguments
dotnet run -- -f "C:\CustomFolder" -p 9090

# Using equals sign
dotnet run -- --folder="C:\CustomFolder" --port=9090
```

The original format is also still supported:
```
dotnet run -- --AppSettings:Folder="C:\CustomFolder" --AppSettings:Port=9090
```

## Running the Built Application

After building the application, you can run the executable directly:

```
MyConsole.exe
```

When running the built application, you can use the same command-line arguments as with `dotnet run`, but without the `--` separator:

```
# Using long form arguments
MyConsole.exe --folder "C:\CustomFolder" --port 9090

# Using short form arguments
MyConsole.exe -f "C:\CustomFolder" -p 9090

# Using equals sign
MyConsole.exe --folder="C:\CustomFolder" --port=9090
```

## Configuration

The application reads configuration from `appsettings.json` by default:

```json
{
  "AppSettings": {
    "Folder": "C:\\DefaultFolder",
    "Port": 8080
  }
}
```

Command line arguments override the settings in the JSON file. The application saves the final configuration values back to the `appsettings.json` file.

## ConfigurationManager Library

This project now uses a separate `ConfigurationManager` library that has been extracted from the original code. The library provides a reusable way to handle configuration and command-line arguments.

### Library Features

- Generic configuration processing with type safety
- Support for multiple configuration sources with precedence
- Command-line argument parsing with both long and short forms
- Automatic saving of settings back to JSON files

### Using the Library in Your Projects

1. Add a reference to the ConfigurationManager project:
   ```xml
   <ProjectReference Include="..\ConfigurationManager\ConfigurationManager.csproj" />
   ```

2. Create a settings class that inherits from `AppSettingsBase`:
   ```csharp
   public class YourSettings : AppSettingsBase
   {
       public string SomeSetting { get; set; }
       public int AnotherSetting { get; set; }
       
       public override string SectionName => "YourSettingsSection";
       
       public override string ToString()
       {
           return $"SomeSetting: {SomeSetting}, AnotherSetting: {AnotherSetting}";
       }
   }
   ```

3. Define command-line option mappings:
   ```csharp
   var optionsMap = new Dictionary<string, (string LongName, string ShortName)>
   {
       { "SomeSetting", ("some-setting", "s") },
       { "AnotherSetting", ("another-setting", "a") }
   };
   ```

4. Create and use the configuration processor:
   ```csharp
   var configProcessor = new ConfigurationProcessor<YourSettings>(
       "your-settings.json",
       optionsMap,
       defaultSettings);
       
   var settings = configProcessor.ProcessConfiguration(args);
   ```

This approach allows you to easily add configuration capabilities to any application with minimal code.

---

This was created in 2 steps using [Windsurf](https://windsurf.com/)

# Plan for Extracting Command-line/Configuration Logic to a Library

## Notes
- The user has published the MyConsole project to GitHub.
- User requested to extract the configuration/command-line argument logic into a separate C# library project for reuse.
- The current implementation supports both long form and short form command-line arguments for configuration.
- We have analyzed AppSettings.cs and Program.cs to identify extractable logic (argument processing, config loading, settings persistence).
- The user has created the ConfigurationManager library project and installed required NuGet packages.
- The core library classes (AppSettingsBase, CommandLineParser, ConfigurationProcessor) have been implemented in the ConfigurationManager project.
- MyConsole now references the ConfigurationManager library and uses it for configuration/command-line logic.

## Task List
- [x] Confirm current command-line/configuration implementation in MyConsole
- [x] Add documentation to README about argument usage
- [x] Publish project to GitHub
- [x] Plan and create a new C# class library project for configuration logic
- [x] Refactor shared logic from MyConsole into the new library
- [x] Reference and use the new library in MyConsole
- [x] Update documentation (README) to reflect library usage

## Current Goal
Project complete


Great! We've successfully completed all the tasks for extracting the configuration functionality into a separate library. Here's a summary of what we've accomplished:
1.	Created a new 
ConfigurationManager
 library project with three main classes:
•	AppSettingsBase
: A base class for settings objects
•	CommandLineParser
: Handles parsing command-line arguments in both long and short forms
•	ConfigurationProcessor
: Core class for loading, processing, and saving configuration
2.	Updated the 
MyConsole
 project to use the new library:
•	Modified 
AppSettings.cs to inherit from 
AppSettingsBase
•	Simplified 
Program.cs to use the 
ConfigurationProcessor
•	Added a project reference to the 
ConfigurationManager
 library
3.	Updated the README.md with documentation on how to use the new library in other projects
The refactored code is now more maintainable, reusable, and follows better separation of concerns. The 
ConfigurationManager
 library can be used in any C# application that needs configuration management with command-line argument support.
Would you like me to explain any specific part of the implementation in more detail?


