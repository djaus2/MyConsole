# MyConsole Application

A C# console application that demonstrates configuration management with the following features:

- Configuration properties:
  - `Folder` (string)
  - `Port` (int)
- Configuration sources (in order of precedence):
  1. Command line arguments
  2. appsettings.json file
- Automatically saves the used configuration values back to appsettings.json

## Building and Running

### Prerequisites
- .NET 6.0 SDK or later

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
