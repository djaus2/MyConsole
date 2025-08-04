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
