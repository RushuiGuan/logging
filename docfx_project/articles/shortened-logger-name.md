# Shortened Logger Name

## Overview
When using `ILogger<T>` in .NET applications, the logger name (also known as the category name in Microsoft.Extensions.Logging or SourceContext in Serilog) defaults to the full type name including namespace. This can result in verbose log output that's harder to read.

Albatross.Logging provides a mechanism to shorten these logger names by removing namespace prefixes, resulting in cleaner, more readable logs.

## The Problem
Consider a typical service class with an injected logger:

```csharp
namespace MyCompany.MyProduct.Services.Orders {
    public class OrderProcessingService {
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(ILogger<OrderProcessingService> logger) {
            _logger = logger;
        }

        public void ProcessOrder(Order order) {
            _logger.LogInformation("Processing order {OrderId}", order.Id);
        }
    }
}
```

The default log output includes the full namespace:
```
2024-01-15 10:30:45 [INF] MyCompany.MyProduct.Services.Orders.OrderProcessingService Processing order 12345
```

With shortened logger names:
```
2024-01-15 10:30:45 [INF] OrderProcessingService Processing order 12345
```

## How It Works
The library provides:
- `IGetLoggerName` interface - Defines how logger names are resolved
- `GetShortenedLoggerNameByNamespacePrefix` - Implementation that shortens names based on namespace prefixes
- `CustomLogger<T>` - A custom `ILogger<T>` implementation that uses the shortened name

## Configuration

### Basic Setup
Register the shortened logger name service in your dependency injection container:

```csharp
public static IServiceCollection AddMyServices(this IServiceCollection services) {
    // Shorten logger names for types in "MyCompany.MyProduct" namespace
    services.AddShortenLoggerName(exclusive: false, "MyCompany.MyProduct");
    return services;
}
```

### Understanding the `exclusive` Parameter
The `exclusive` parameter controls which types get shortened names:

**When `exclusive = false` (default behavior):**
- Types whose namespace **starts with** the specified prefix → **shortened** to class name only
- Types whose namespace **does not start with** the prefix → **full namespace** preserved

```csharp
// exclusive = false, prefix = "MyCompany"
services.AddShortenLoggerName(false, "MyCompany");

// MyCompany.Services.OrderService → "OrderService" (shortened)
// Microsoft.Extensions.Hosting.Host → "Microsoft.Extensions.Hosting.Host" (full)
```

**When `exclusive = true`:**
- Types whose namespace **starts with** the specified prefix → **full namespace** preserved
- Types whose namespace **does not start with** the prefix → **shortened** to class name only

```csharp
// exclusive = true, prefix = "Microsoft"
services.AddShortenLoggerName(true, "Microsoft");

// Microsoft.Extensions.Hosting.Host → "Microsoft.Extensions.Hosting.Host" (full)
// MyCompany.Services.OrderService → "OrderService" (shortened)
```

### Multiple Namespace Prefixes
You can specify multiple namespace prefixes:

```csharp
services.AddShortenLoggerName(false, "MyCompany.ProductA", "MyCompany.ProductB", "MyCompany.Shared");
```

## Complete Example

### Program.cs / Startup.cs
```csharp
using Albatross.Logging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) => {
    // Register your services
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<IInventoryService, InventoryService>();

    // Shorten logger names for your application namespaces
    services.AddShortenLoggerName(false, "MyCompany.MyProduct");
});

// Configure Serilog
builder.UseSerilog((context, config) => {
    config.WriteTo.Console(outputTemplate:
        "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
});
```

### Service Class
```csharp
namespace MyCompany.MyProduct.Services {
    public class OrderService : IOrderService {
        private readonly ILogger<OrderService> _logger;

        public OrderService(ILogger<OrderService> logger) {
            _logger = logger;
        }

        public async Task ProcessOrderAsync(Order order) {
            _logger.LogInformation("Starting order processing for {OrderId}", order.Id);
            // ... processing logic
            _logger.LogInformation("Completed order processing for {OrderId}", order.Id);
        }
    }
}
```

### Output
```
10:30:45 [INF] OrderService: Starting order processing for 12345
10:30:46 [INF] OrderService: Completed order processing for 12345
```

## API Reference

### IGetLoggerName Interface
```csharp
public interface IGetLoggerName {
    string Get(Type type);
}
```

### GetShortenedLoggerNameByNamespacePrefix Class
```csharp
public class GetShortenedLoggerNameByNamespacePrefix : IGetLoggerName {
    public GetShortenedLoggerNameByNamespacePrefix(bool exclusive, string[] namespacePrefixes);
    public string Get(Type type);
}
```

### Extension Method
```csharp
public static IServiceCollection AddShortenLoggerName(
    this IServiceCollection services,
    bool exclusive,
    params string[] namespacePrefix);
```

## See Also
- [ErrorMessage Enricher](error-msg-enricher.md) - Add exception messages inline with log entries
- [Enhanced SlackSink](enhanced-slacksink.md) - Configure Slack notifications