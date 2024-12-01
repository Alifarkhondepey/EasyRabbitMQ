
# EasyRabbitMQ.Net

**EasyRabbitMQ.Net** is a .NET library that simplifies RabbitMQ messaging for producers and consumers. It provides seamless integration with RabbitMQ Client Version 7 and supports modern .NET 8 dependency injection practices.

---

## Features

- **Producer and Consumer Support**: Simplifies RabbitMQ messaging.
- **Dependency Injection Ready**: Fully compatible with `.NET 8` DI patterns.
- **Asynchronous Operations**: Supports async RabbitMQ interactions.
- **Customizable Factory Methods**: For advanced RabbitMQ setup.
- **Error Handling**: Provides robust error handling and logging.

---

## Installation

### Step 1: Install the Package

Install **EasyRabbitMQ.Net** from NuGet:

```bash
dotnet add package EasyRabbitMQ.Net
```

Or visit the [NuGet Package](https://www.nuget.org/packages/EasyRabbitMQ.Net).

---

## Configuration

### Step 2: Add RabbitMQ Settings

Define RabbitMQ settings in `appsettings.json`:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Port": 5672
  }
}
```

---

## Usage

### Consumer Application

For consuming messages, update `Program.cs` as follows:

#### **Consumer Program.cs**

```csharp
using EasyRabbitMQ.Net.RabbitMQ.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Load RabbitMQ configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure RabbitMQ services
builder.Services.AddRabbitMQServices(builder.Configuration);

// Add logging (optional)
builder.Services.AddLogging();

await builder.Build().RunAsync();
```

---

### Producer Application

For producing messages, update `Program.cs` as follows:

#### **Producer Program.cs**

```csharp
using EasyRabbitMQ.Net.RabbitMQ.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ services
builder.Services.AddRabbitMQServices(builder.Configuration);

// Add logging (optional)
builder.Services.AddLogging();

await builder.Build().RunAsync();
```

---

## Advanced Usage

### Factory Method for Custom Initialization

For advanced RabbitMQ initialization, you can use the **RabbitMQFactory** class:

```csharp
using EasyRabbitMQ.Net.RabbitMQ.Models;
using EasyRabbitMQ.Net.Consumer;

var factory = new RabbitMQFactory(configuration, loggerFactory);

var producer = await factory.CreateProducer();
var consumer = await factory.CreateConsumerAsync();
```

---

## Dependency Injection Extension

You can use the `AddRabbitMQServices` extension method to register RabbitMQ services easily. This method registers:

- RabbitMQ configuration (`RabbitMQSettings`)
- Producers (`IMessageProducer`)
- Consumers (`IMessageConsumer`)
- RabbitMQ connection and channel lifecycle management

```csharp
services.AddRabbitMQServices(configuration);
```

---

## Sample Projects

### Producer Application

This sample demonstrates how to send messages using `EasyRabbitMQ.Net`. Refer to the **Producer Program.cs** above.

### Consumer Application

This sample demonstrates how to consume messages using `EasyRabbitMQ.Net`. Refer to the **Consumer Program.cs** above.

---

## Changelog

### Version 2.0.0

- Updated to RabbitMQ Client Version 7.
- Support for .NET 8 hosting and dependency injection.
- Enhanced consumer and producer functionality.
- Simplified RabbitMQ configuration and setup.

---

## Contributing

Contributions are welcome! Visit the [GitHub repository](https://github.com/Alifarkhondepey/EasyRabbitMQ) for more information on how to contribute.

---

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/Alifarkhondepey/EasyRabbitMQ?tab=MIT-1-ov-file#readme) file for details.
