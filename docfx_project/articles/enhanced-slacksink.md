# Enhanced SlackSink

## Overview
Albatross.Logging provides enhanced support for the Serilog Slack sink, addressing common configuration challenges when deploying applications across multiple servers with shared environment variables.

## The Problem
The standard Slack sink configuration stores the webhook URL in `SlackSinkOptions`:

```json
{
  "Serilog": {
    "WriteTo": {
      "SlackSink": {
        "Name": "Slack",
        "Args": {
          "SlackSinkOptions": {
            "WebHookUrl": "https://hooks.slack.com/services/...",
            "BatchSizeLimit": 20,
            "ShowDefaultAttachments": false,
            "ShowPropertyAttachments": false,
            "ShowExceptionAttachments": false
          },
          "restrictedToMinimumLevel": "Error",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} {MachineName} {SourceContext} [{Level:w3}] {Message:lj}"
        }
      }
    }
  }
}
```

Since the webhook URL is a secret, it's typically stored as an environment variable. The Serilog configuration system expects:
```
Serilog__WriteTo__SlackSink__Args__SlackSinkOptions__WebHookUrl=https://hooks.slack.com/services/...
```

**The problem:** This environment variable is global on the server. Any application using Serilog configuration (even without Slack sink) will try to partially initialize `SlackSinkOptions` and fail because other required options are missing.

## Solutions

### Solution 1: Single App Per Server
Only run one application per server. This is often not practical in production environments.

### Solution 2: Remove Legacy Environment Variable
For applications that don't use the Slack sink, call `RemoveLegacySlackSinkOptions()` at startup to clear the problematic environment variable:

```csharp
using Albatross.Logging;

// Call this before Serilog initialization
Extensions.RemoveLegacySlackSinkOptions();

// Now configure Serilog normally
var logger = new SetupSerilog()
    .UseConfigFile(environment, null, null, true)
    .UseConsole(LogEventLevel.Information)
    .Create();
```

This removes the `Serilog__WriteTo__SlackSink__Args__SlackSinkOptions__WebHookUrl` environment variable for the current process.

### Solution 3: Use SinkOptions.SlackSink (Recommended)
Use the `SinkOptions.SlackSink` static property which reads the webhook URL from a dedicated environment variable `SlackSinkWebHookUrl`:

**Step 1:** Set the environment variable on your server:
```bash
# Linux/macOS
export SlackSinkWebHookUrl="https://hooks.slack.com/services/..."

# Windows
setx SlackSinkWebHookUrl "https://hooks.slack.com/services/..."
```

**Step 2:** Configure serilog.json to use the SinkOptions provider:
```json
{
  "Serilog": {
    "Using": [
      "Albatross.Logging"
    ],
    "WriteTo": {
      "SlackSink": {
        "Name": "Slack",
        "Args": {
          "SlackSinkOptions": "Albatross.Logging.SinkOptions::SlackSink, Albatross.Logging",
          "restrictedToMinimumLevel": "Error",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} {MachineName} {SourceContext} [{Level:w3}] {Message:lj}{ErrorMessage}"
        }
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithThreadId",
      "WithMachineName",
      "WithErrorMessage"
    ]
  }
}
```

The `"Albatross.Logging.SinkOptions::SlackSink, Albatross.Logging"` syntax tells Serilog to use the static `SlackSink` property from the `SinkOptions` class.

### Solution 4: Mixed Approach
On servers with both legacy and new applications:
1. Keep the legacy environment variable for older apps
2. Use `RemoveLegacySlackSinkOptions()` in apps that don't need Slack
3. Use `SinkOptions.SlackSink` in apps that need Slack with the new configuration

## SinkOptions Configuration
The `SinkOptions.SlackSink` property provides these default settings:

| Setting | Value |
|---------|-------|
| WebHookUrl | From `SlackSinkWebHookUrl` environment variable |
| BatchSizeLimit | 20 |
| ShowDefaultAttachments | false |
| ShowPropertyAttachments | false |
| ShowExceptionAttachments | false |

## Complete Example
Here's a complete `serilog.json` configuration using the enhanced Slack sink with the ErrorMessage enricher:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Using": [
      "Albatross.Logging"
    ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} [{Level:w3}] {SourceContext} {Message:lj}{ErrorMessage}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app.log",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} [{Level:w3}] {SourceContext} {Message:lj}{ErrorMessage}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Slack",
        "Args": {
          "SlackSinkOptions": "Albatross.Logging.SinkOptions::SlackSink, Albatross.Logging",
          "restrictedToMinimumLevel": "Error",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} {MachineName} {SourceContext} [{Level:w3}] {Message:lj}{ErrorMessage}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithThreadId",
      "WithMachineName",
      "WithErrorMessage"
    ]
  }
}
```

## API Reference

### Extensions.RemoveLegacySlackSinkOptions()
Removes the `Serilog__WriteTo__SlackSink__Args__SlackSinkOptions__WebHookUrl` environment variable for the current process.

```csharp
public static void RemoveLegacySlackSinkOptions();
```

### SinkOptions.SlackSink
Static property that returns a `SlackSinkOptions` instance configured with the webhook URL from the `SlackSinkWebHookUrl` environment variable.

```csharp
public static SlackSinkOptions SlackSink { get; }
```

## See Also
- [ErrorMessage Enricher](error-msg-enricher.md) - Add exception messages inline with log entries (great for Slack notifications)
- [Shortened Logger Name](shortened-logger-name.md) - Reduce verbose logger names in output