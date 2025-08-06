# Sportronics.ConfigurationManager

A flexible configuration management library for .NET applications that handles settings from JSON files and command line arguments.

## Features

- Load configuration from JSON files
- Parse and apply command line arguments
- Combine multiple configuration sources with priority order
- Save configuration changes back to JSON files
- Support for both long and short command line argument formats
- Reset configuration to defaults with a simple command line flag
- Bypass stored settings with ignore flag to use in-app or default settings
- Built-in help system with command line options documentation

## NuGet Package

**Sportronics.ConfigurationManager**

## Installation

```bash
dotnet add package Sportronics.ConfigurationManager
```

## Usage

Create a C# Console app that uses the `Sportronics.ConfigurationManager`. Library is straightforward. Below is a step-by-step guide to set up your application with configuration management.

### 0. Getting Started
After creating the project add the NuGet package as above or similar.

### 1. Create a settings class AppSettings.cs

```csharp
using Sportronics.ConfigurationManager;

public class MyAppSettings : AppSettingsBase
{
    public override string SectionName => "MyApp";
    
    public string InputFolder { get; set; } = "default/input";
    public string OutputFolder { get; set; } = "default/output";
    public bool VerboseLogging { get; set; } = false;
    
    public override string ToString()
    {
        return $"InputFolder: {InputFolder}, OutputFolder: {OutputFolder}, VerboseLogging: {VerboseLogging}";
    }
}
```

### 2. Declare in-app  settings entity
```csharp
using Sportronics.ConfigurationManager;

...
...

AppSettings? defaultSettings = null;
```

### 3. Optionally set in-app settings
```csharp
defaultSettings = new MyAppSettings
{
    InputFolder = "in-app/input",
    OutputFolder = "in-app/output",
    VerboseLogging = true
};
```

### 4. Define command line options

```csharp
var optionsMap = new Dictionary<string, (string LongName, string ShortName)>
{
    { nameof(MyAppSettings.InputFolder), ("input", "i") },
    { nameof(MyAppSettings.OutputFolder), ("output", "o") },
    { nameof(MyAppSettings.VerboseLogging), ("verbose", "v") }
};
```

### 5. Process configuration

```csharp
// Initialize the configuration processor
var configProcessor = new ConfigurationProcessor<AppSettings>(
    "appsettings.json",
    optionsMap,
    defaultSettings);

// Process configuration from all sources
var settings = configProcessor.ProcessConfiguration(args);
if (settings == null)
{
    // Help was displayed, exit the application
    return;
}

// Use the settings
Console.WriteLine($"Using settings: {settings}");
```

### 4. Reset to default settings

Users can reset all settings to their default values by using the `-r` or `--reset` command line flag:

```bash
# Using short form
MyApp.exe -r

# Using long form
MyApp.exe --reset
```

This will reset all settings to their default values as defined in your settings class and save them to the JSON configuration file.

### 5. Ignore stored settings

Users can bypass the stored settings in the JSON file and use in-app settings (or defaults if no in-app settings are available) by using the `-i` or `--ignore` command line flag:

```bash
# Using short form
MyApp.exe -i

# Using long form
MyApp.exe --ignore
```

This is useful when you want to ensure that the application uses the in-app settings or defaults, ignoring any previously stored configuration in the JSON file.

### 6. Display help information

Users can display help information about available command line options by using the `-h` or `--help` flag:

```bash
# Using short form
MyApp.exe -h

# Using long form
MyApp.exe --help
```

This will display information about all available command line options, including both special flags and registered configuration options.

When the help flag is used, the `ProcessConfiguration` method will return `null`, allowing the application to exit early.

## License

This project is licensed under the Creative Commons Attribution 4.0 International License - see the LICENSE file for details.

## Release Notes

### v1.1.X (August 2025)
- 1.1.4 Repository renamed so fixed link.
- 1.1.3 Fixed issue with command line argument parsing
- 1.1.2 Fixed repository link
- 1.1.1 Embelished README with more detailed usage instructions
- 1.1.0 Added support for resetting configuration to default values via command line flag `-r` or `--reset`)
  - Added support for ignoring stored settings via command line flag `-i` or `--ignore`
  - Added help command line flag `-h` or `--help` to display available options

### v1.0.3 (August 2025)
- v1.0.3 - Added project icon

### v1.0.2 (August 2025)
- Renamed project from ConfigurationManager to Sportronics.ConfigurationManager
- Improved namespace organization
- Added support for both long and short form command line arguments
- Fixed bug in configuration saving

### v1.0.1
- Initial public release
- Basic configuration management functionality
- Support for JSON files and command line arguments
