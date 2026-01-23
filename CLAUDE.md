# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build logging.sln

# Build a specific project
dotnet build Albatross.Logging/Albatross.Logging.csproj

# Build in Release mode
dotnet build logging.sln -c Release

# Create NuGet package
dotnet pack Albatross.Logging/Albatross.Logging.csproj -c Release
```

## Project Structure

This is a .NET logging library built on Serilog, published as the `Albatross.Logging` NuGet package.

- **Albatross.Logging** - Core library (netstandard2.0) providing Serilog setup utilities
- **Sample.CommandLine** - Command-line sample app (net9.0) demonstrating library usage
- **Sample.WebApi** - Web API sample app (net9.0) demonstrating library usage with ASP.NET Core

## Architecture

The library provides a fluent builder pattern for Serilog configuration via `SetupSerilog`:

```csharp
var logger = new SetupSerilog()
    .UseConfigFile(environment, basePath, args)  // Load serilog.json config
    .UseConsole(LogEventLevel.Information)        // Add console sink
    .Configure(cfg => { /* custom config */ })    // Custom configuration
    .Create();
```

Key components:
- `SetupSerilog` - Main entry point for logger configuration
- `ErrorMessageEnricher` - Serilog enricher that adds error message properties
- `CustomLogger<T>` / `GetShortenedLoggerNameByNamespacePrefix` - Custom ILogger implementation that shortens logger names

## Code Style

- Uses tabs for indentation (tab width: 4)
- Opening braces on same line as declaration (K&R style)
- No final newline at end of files
- Explicit types preferred over `var`
