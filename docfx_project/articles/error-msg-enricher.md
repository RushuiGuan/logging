# ErrorMessage Enricher

## Overview
The `ErrorMessageEnricher` is a Serilog enricher that extracts the `Message` property from exceptions and adds it as a separate log property. This allows exception messages to appear inline with log entries, providing immediate visibility into errors without parsing the full stack trace.

## Why Use It?
- **Quick Error Identification** - See exception messages at a glance without scrolling through stack traces
- **Better Monitoring** - Useful for critical applications where quick error identification is essential
- **Slack Integration** - When combined with SlackSink, provides meaningful error notifications with the exception message prominently displayed

## How It Works
The enricher creates a property named `ErrorMessage` that contains:
- The exception's `Message` property prefixed with ` - ` when an exception exists
- An empty string when no exception is present

This allows the error message to naturally flow in your log output template.

## Configuration

### Step 1: Add to Enrichers
Add `WithErrorMessage` to your Serilog enrichers list in `serilog.json`:

```json
{
  "Serilog": {
    "Using": [
      "Albatross.Logging"
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

### Step 2: Add to Output Template
Include `{ErrorMessage}` in your output template where you want the exception message to appear:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} [{Level:w3}] {SourceContext} {Message:lj}{ErrorMessage}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

## Complete Example
Here's a complete `serilog.json` configuration using the ErrorMessage enricher with both console and Slack sinks:

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

## Programmatic Configuration
You can also configure the enricher programmatically using the extension method:

```csharp
using Albatross.Logging;

var logger = new LoggerConfiguration()
    .Enrich.WithErrorMessage()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{ErrorMessage}{NewLine}{Exception}")
    .CreateLogger();
```

## Sample Output
Without ErrorMessage enricher:
```
2024-01-15 10:30:45-05:00 [ERR] MyApp.Services.OrderService Failed to process order
System.InvalidOperationException: Order validation failed
   at MyApp.Services.OrderService.ProcessOrder(Order order)
```

With ErrorMessage enricher:
```
2024-01-15 10:30:45-05:00 [ERR] MyApp.Services.OrderService Failed to process order - Order validation failed
System.InvalidOperationException: Order validation failed
   at MyApp.Services.OrderService.ProcessOrder(Order order)
```

## See Also
- [Enhanced SlackSink](enhanced-slacksink.md) - Configure Slack notifications with ErrorMessage enricher