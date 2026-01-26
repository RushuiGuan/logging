# Release Notes

## 10.0.3

### Bug Fixes
- **UseConsole sets global MinimumLevel to Verbose**: `UseConsole` now explicitly sets `MinimumLevel.Verbose()` on the `LoggerConfiguration` so that the console sink's `LoggingLevelSwitch` has full control over filtering. Other sinks should use their own `restrictedToMinimumLevel` or a sub-logger to control their log level independently.

### Documentation
- Added XML documentation comments to both `UseConsole` overloads, documenting behavior and the side effect of setting the global minimum level to `Verbose`.

## 10.0.2

### Deprecations
- `SetupSerilog.UseConfigFile(string, string?, string[]?)` is now marked as `[Obsolete]`. Use the new overload with the `optional` parameter instead.

### New Features
- **Optional serilog.json configuration**: Added `UseConfigFile(string environment, string? basePath, string[]? commandLineArgs, bool optional)` overload that allows `serilog.json` to be optional. When `optional` is `true`, the application will not fail if the config file is missing.

### Bug Fixes
- **Independent console log level**: Fixed `UseConsole` to use `levelSwitch` on the Console sink instead of setting the global `MinimumLevel`. This allows file-based configuration (from `serilog.json`) to use its own minimum level independently from the console sink level.

### Documentation
- Enhanced README with Summary, Features, Quick Start, Code Repo, and Documentation sections
- Added comprehensive docfx documentation site
- Enhanced articles for ErrorMessage Enricher, Shortened Logger Name, and Enhanced SlackSink

## 10.0.1

- Updated Serilog.Extensions.Hosting to version 10.0.0