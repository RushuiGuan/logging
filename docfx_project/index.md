# Albatross.Logging
Albatross.Logging is a .NET library that provides quick and easy logging setup for .NET applications using Serilog. It offers a fluent builder pattern for configuring Serilog with support for file-based configuration, console output, and various sinks. The library targets .NET Standard 2.0, making it compatible with .NET Framework 4.6.1+ and .NET Core/.NET 5+.

## Features
- **Fluent Configuration API** - Simple builder pattern via `SetupSerilog` class for configuring Serilog
- **File-Based Configuration** - Load Serilog settings from `serilog.json` with environment-specific overrides
- **Console Sink with Dynamic Level Control** - Built-in console sink with runtime log level switching
- **[ErrorMessage Enricher](https://rushuiguan.github.io/logging/articles/error-msg-enricher.html)** - Display exception messages inline with log entries for quick error identification
- **[Shortened Logger Name](https://rushuiguan.github.io/logging/articles/shortened-logger-name.html)** - Reduce verbose namespace-based logger names for cleaner output
- **[Enhanced SlackSink](https://rushuiguan.github.io/logging/articles/enhanced-slacksink.html)** - Improved Slack integration with flexible webhook URL configuration

## Quick Start
Install the package:
```bash
dotnet add package Albatross.Logging
```

Basic setup with code configuration:
```csharp
var logger = new SetupSerilog()
    .UseConsole(LogEventLevel.Information)
    .Create();
```

Setup with file-based configuration (`serilog.json`):
```csharp
var logger = new SetupSerilog()
    .UseConfigFile(environment, basePath, args, optional: true)
    .UseConsole(LogEventLevel.Information)
    .Create();
```

Integrate with Microsoft.Extensions.Hosting:
```csharp
var builder = Host.CreateDefaultBuilder();
builder.UseSerilog();
builder.ConfigureLogging((context, logging) => {
    new SetupSerilog()
        .UseConfigFile(environment, null, null, true)
        .UseConsole(LogEventLevel.Information)
        .Create();
});
```

Sample `serilog.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "System": "Information",
        "Microsoft": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "log\\log.txt",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} [{Level:w3}] ({ThreadId}) {SourceContext} {Message:lj}{NewLine}{Exception}",
          "rollingInterval": "Day"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithThreadId" ]
  }
}
```

## Code Repo
[https://github.com/RushuiGuan/logging](https://github.com/RushuiGuan/logging)

## NuGet
[https://www.nuget.org/packages/Albatross.Logging](https://www.nuget.org/packages/Albatross.Logging)