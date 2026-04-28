A comprehensive, enterprise-grade SignalR client library for .NET that provides robust connection management, advanced resilience patterns, and production-ready observability.

![Platform](https://img.shields.io/badge/Platform-.NET%208%2B-purple?logo=dotnet) ![License](https://img.shields.io/badge/License-MIT-lightgrey?logo=opensourceinitiative) ![Production Ready](https://img.shields.io/badge/Production-Ready-brightgreen?logo=check-circle) ![Performance](https://img.shields.io/badge/Performance-⚡%20High%20Throughput-ff6b6b) ![Developer Friendly](https://img.shields.io/badge/Developer%20Friendly-😊%20Fluent%20API-51cf66)

![NuGet Version](https://img.shields.io/nuget/v/Shaunebu.Common.SignalRManager?color=blue&label=NuGet) ![Downloads](https://img.shields.io/nuget/dt/Shaunebu.Common.SignalRManager?color=green&label=Downloads)

![NET Support](https://img.shields.io/badge/.NET%20MAUI-%3E%3D8.0-512BD4?logo=dotnet) ![NET Core](https://img.shields.io/badge/.NET%20Core-%3E%3D3.1-blueviolet) ![CrossPlatform](https://img.shields.io/badge/Platforms-Android%20%7C%20iOS%20%7C%20Windows%20%7C%20macOS-00BFFF)
![Architecture](https://img.shields.io/badge/Architecture-Resilience%20%7C%20DI%20%7C%20Telemetry-blueviolet)

![Resilience](https://img.shields.io/badge/Resilience-Polly%20%7C%20Circuit%20Breaker-orange) ![Observability](https://img.shields.io/badge/Observability-%F0%9F%93%88%20Metrics%20%7C%20Logging-success) ![Offline](https://img.shields.io/badge/Offline%20Queue-✅%20Persistent-green) ![Hub Generator](https://img.shields.io/badge/Hub%20Generator-🎯%20Type--Safe%20APIs-blue)

[![Support](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/jcz65te)

✨ Features
----------

### 🔌 Connection Management

*   **Automatic Reconnection** - Handles transient failures gracefully
    
*   **Connection State Tracking** - Real-time state monitoring with events
    
*   **Configurable Timeouts** - Customizable connection and handshake timeouts
    
*   **Multi-Transport Support** - WebSockets, Server-Sent Events, Long Polling
    

### 📨 Message Processing

*   **Intelligent Batching** - Configurable batch sizes and intervals
    
*   **Priority Queuing** - Priority-based message processing
    
*   **Offline Queue** - Message persistence during disconnections
    
*   **Batch Processing** - Efficient bulk message delivery
    

### 🛡️ Resilience & Reliability

*   **Polly Integration** - Advanced retry policies with exponential backoff
    
*   **Circuit Breaker** - Prevents cascade failures
    
*   **Error Isolation** - Handler errors don't break the system
    
*   **Graceful Degradation** - Maintains partial functionality during failures
    

### 📊 Observability

*   **Comprehensive Metrics** - Connection, message, and performance metrics
    
*   **Structured Logging** - Detailed logging with configurable levels
    
*   **State Change Events** - Real-time connection state notifications
    
*   **Custom Exporters** - Extensible metrics exporting
    

### 🔧 Developer Experience

*   **Type-Safe Wrappers** - Compile-time safety for hub methods
    
*   **Fluent Configuration** - Intuitive builder pattern setup
    
*   **Dependency Injection** - Seamless [ASP.NET](https://asp.net/) Core integration
    
*   **Async/Await Support** - Full asynchronous operation support
    

🚀 Quick Start
--------------

### Installation

```bash
dotnet add package Shaunebu.SignalRManager
```

### Type-Safe Usage (Recommended)

> **Requires**: [Shaunebu.SignalR.HubGenerator](../Shaunebu.SignalR.HubGenerator) source generator and `[ServerMethod]`/`[ClientMethod]` attributes on your hub methods.

```csharp
// 1. Create and configure the manager
var manager = new SignalRManagerBuilder()
    .WithHubUrl("https://api.example.com/hubs/chat")
    .WithBatching(20, TimeSpan.FromMilliseconds(250))
    .WithPollyRetryPolicy(5)
    .WithMetricsExporter(new ConsoleMetricsExporter())
    .WithAutoReconnect(true, 3)
    .Build();

// 2. Initialize connection
await manager.InitializeAsync();

// 3. Register type-safe handler (no magic strings)
manager.RegisterHandler<object, string, string>(
    nameof(IChatHubClient.ReceiveMessage),
    (user, msg) => Console.WriteLine($"[Type-Safe Chat] {user}: {msg}"));

// 4. Type-safe method invocation (no parameter duplication)
await manager.InvokeAsync<IChatHubServer>(
    s => s.SendMessage("John", "Hello World!"));

// 5. Clean shutdown
await manager.ShutdownAsync();
```

📖 Comprehensive Examples
-------------------------

### Type-Safe Hub Clients

```csharp
// Create type-safe client wrapper
public class ChatHubClient
{
    private readonly ISignalRManager _manager;
    
    public ChatHubClient(ISignalRManager manager) => _manager = manager;
    
    public Task SendMessageAsync(string user, string message) =>
        _manager.InvokeAsync<object>("SendMessage", new object[] { user, message });
    
    public IDisposable OnMessageReceived(Action<string, string> handler) =>
        _manager.RegisterHandler<string, string>("ReceiveMessage", handler);
}

// Usage
var chatClient = new ChatHubClient(manager);
await chatClient.SendMessageAsync("Alice", "Hello from type-safe client!");
chatClient.OnMessageReceived((user, msg) => Console.WriteLine($"{user}: {msg}"));
```

### Advanced Configuration

```csharp
var manager = new SignalRManagerBuilder()
    .WithHubUrl("https://api.example.com/hubs/notifications")
    // Resilience
    .WithPollyRetryPolicy(
        maxRetries: 5, 
        initialDelay: TimeSpan.FromSeconds(1),
        maxDelay: TimeSpan.FromMinutes(2))
    .WithCircuitBreakerPersistence(new RedisCircuitBreakerPersistence())
    // Performance
    .WithBatching(batchSize: 50, batchInterval: TimeSpan.FromMilliseconds(500))
    .WithOfflineQueue(enabled: true)
    // Security
    .WithEncryptor(new AesEncryptor(key, iv))
    .WithHeader("Authorization", "Bearer your-token")
    // Observability
    .WithMetricsExporter(new ApplicationInsightsMetricsExporter())
    .WithLogger(logger)
    // Advanced
    .WithTransport(TransportType.WebSockets)
    .WithTimeout(connectionTimeout: TimeSpan.FromSeconds(30))
    .ConfigureConnection(builder => 
        builder.WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2) }))
    .Build();
```

### [ASP.NET](https://asp.net/) Core Integration

```csharp
// Program.cs
builder.Services.AddSignalRManager(options =>
{
    options.HubUrl = "https://api.example.com/hubs/chat";
    options.EnableBatching = true;
    options.BatchSize = 20;
    options.EnableOfflineQueue = true;
    options.AutoReconnect = true;
})
.AddTypedHubClients(); // Optional: for type-safe clients

// Controller usage
public class NotificationController(ISignalRManager signalRManager)
{
    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        await signalRManager.InvokeAsync("SendNotification", new object[] { request.Message });
        return Ok();
    }
}
```

🔧 Configuration Options
------------------------

### SignalRManagerOptions

| Property | Default | Description |
| --- | --- | --- |
| `HubUrl` | Required | SignalR hub URL |
| `EnableBatching` | `true` | Enable message batching |
| `BatchSize` | `10` | Maximum messages per batch |
| `BatchInterval` | `250ms` | Maximum time to wait for batch |
| `EnableOfflineQueue` | `true` | Persist messages when offline |
| `HeartbeatInterval` | `30s` | Heartbeat check interval |
| `AutoReconnect` | `true` | Automatic reconnection |
| `MaxReconnectAttempts` | `5` | Maximum reconnection attempts |
| `ConnectionTimeout` | `30s` | Connection establishment timeout |
| `TransportType` | `All` | Preferred transport type |

📊 Monitoring & Metrics
-----------------------

### Built-in Metrics

```csharp
public interface IMetricsExporter
{
    void OnConnected(string connectionId);
    void OnDisconnected(string? connectionId);
    void OnReconnectAttempt(int attempt);
    void OnMessageSent();
    void OnMessageBatchProcessed(int batchSize);
    void OnCircuitBreakerStateChange(CircuitBreakerState state);
    void OnError(Exception exception, string operation);
}
```

### Custom Metrics Exporter

```csharp
public class CustomMetricsExporter : IMetricsExporter
{
    public void OnConnected(string connectionId)
    {
        // Integrate with your monitoring system
        Metrics.Increment("signalr.connections.active");
        Logger.LogInformation("Client connected: {ConnectionId}", connectionId);
    }
    
    // Implement other methods...
}
```

🛡️ Resilience Patterns
-----------------------

### Custom Retry Policy

```csharp
public class CustomRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextDelay(int attempt, Exception? lastException)
    {
        if (IsFatalException(lastException)) return null;
        
        return attempt switch
        {
            < 3 => TimeSpan.FromSeconds(1 * attempt),
            < 5 => TimeSpan.FromSeconds(5),
            _ => null // Stop retrying
        };
    }
    
    private bool IsFatalException(Exception? ex) => 
        ex is OperationCanceledException or ObjectDisposedException;
}
```

### Circuit Breaker with Persistence

```csharp
// Redis persistence example
public class RedisCircuitBreakerPersistence : ICircuitBreakerPersistence
{
    private readonly IDatabase _redis;
    
    public async Task<CircuitState?> LoadStateAsync(string circuitName) =>
        await _redis.StringGetAsync($"circuitbreaker:{circuitName}");
    
    public async Task SaveStateAsync(string circuitName, CircuitState state) =>
        await _redis.StringSetAsync($"circuitbreaker:{circuitName}", state);
}
```


📊 Feature Comparison Table
---------------------------

| Feature | SignalRManager | Microsoft SignalR Client | Azure SignalR Service SDK | SignalR.Client.Core |
| --- | --- | --- | --- | --- |
| **Connection Management** | ✅ **Advanced** | ✅ Basic | ✅ Managed | ✅ Basic |
| **Automatic Reconnection** | ✅ **Configurable** | ✅ Limited | ✅ Managed | ✅ Limited |
| **Message Batching** | ✅ **Intelligent** | ❌ None | ❌ None | ❌ None |
| **Priority Queuing** | ✅ **Yes** | ❌ No | ❌ No | ❌ No |
| **Offline Queue** | ✅ **Persistent** | ❌ No | ❌ No | ❌ No |
| **Retry Policies** | ✅ **Polly + Custom** | ✅ Basic | ✅ Basic | ✅ Basic |
| **Circuit Breaker** | ✅ **Yes** | ❌ No | ❌ No | ❌ No |
| **State Persistence** | ✅ **Yes** | ❌ No | ❌ No | ❌ No |
| **Metrics & Telemetry** | ✅ **Comprehensive** | ❌ Minimal | ✅ Limited | ❌ No |
| **Type-Safe Wrappers** | ✅ **Yes** | ❌ No | ❌ No | ❌ No |
| **DI Integration** | ✅ **First-class** | ✅ Basic | ✅ Basic | ✅ Basic |
| **Configuration** | ✅ **Fluent API** | ✅ Basic | ✅ Basic | ✅ Basic |
| **Performance** | ✅ **Optimized** | ✅ Good | ✅ Good | ✅ Good |
| **Learning Curve** | 🟡 Moderate | 🟢 Easy | 🟢 Easy | 🟢 E |


🔍 Troubleshooting
------------------

### Common Issues

**Connection Failures**

```csharp
// Enable detailed logging
var manager = new SignalRManagerBuilder()
    .WithHubUrl("your-hub-url")
    .WithLogger(LoggerFactory.Create(builder => 
        builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
    .Build();
```

**Message Delivery Issues**

```csharp
// Monitor batch events
manager.OnMessageBatched += (method, count) => 
    Console.WriteLine($"{count} messages batched for {method}");

// Check offline queue
manager.OnOfflineQueueSizeChanged += size =>
    Console.WriteLine($"Offline queue size: {size}");
```


📈 Performance Tips
-------------------

1.  **Batch Sizing**: Adjust `BatchSize` and `BatchInterval` based on your throughput requirements
    
2.  **Priority Queuing**: Use message priorities for critical messages
    
3.  **Connection Pooling**: Reuse SignalRManager instances when possible
    
4.  **Memory Management**: Monitor offline queue size in high-volume scenarios
    


📄 License
----------

This project is licensed under the MIT License.

🏆 Acknowledgments
------------------

*   Built on top of Microsoft's [SignalR Client](https://github.com/dotnet/aspnetcore)
    
*   Resilience patterns inspired by [Polly](https://github.com/App-vNext/Polly)
    
*   Metrics design influenced by OpenTelemetry standards




<br><br>

🎯 Hub Generator - Type-Safe SignalR Development
------------------------------------------------

### ⚡ Automatic Interface Generation

SignalRManager includes a powerful **Source Generator** that automatically creates strongly-typed interfaces for ALL classes that inherit from `Hub`, eliminating magic strings and providing compile-time safety.

### 🔧 How It Works

#### 1. Simply Inherit from Hub (No Attributes Needed!)

```csharp
// No attributes required! The generator automatically detects Hub classes
public class ChatHub : Hub
{
    [ServerMethod]
    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    
    [ServerMethod]
    public Task JoinGroup(string groupName)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
    
    [ClientMethod]
    private Task ReceiveMessage(string user, string message) => Task.CompletedTask;
    
    [ClientMethod]
    private Task UserJoined(string user) => Task.CompletedTask;
}

public class NotificationHub : Hub
{
    [ServerMethod]
    public Task SendNotification(string message)
    {
        return Clients.All.SendAsync("ReceiveNotification", message);
    }
    
    [ClientMethod]
    private Task ReceiveNotification(string message) => Task.CompletedTask;
}
```

#### 2. Generated Interfaces (Automatically Created)

**IChatHubServer.g.cs** (Server-side interface):

```csharp
public interface IChatHubServer
{
    Task SendMessage(string user, string message);
    Task JoinGroup(string groupName);
}
```

**IChatHubClient.g.cs** (Client-side interface):

```csharp
public interface IChatHubClient
{
    Task ReceiveMessage(string user, string message);
    Task UserJoined(string user);
}
```

**INotificationHubServer.g.cs**:

```csharp
public interface INotificationHubServer
{
    Task SendNotification(string message);
}
```

**INotificationHubClient.g.cs**:

```csharp
public interface INotificationHubClient
{
    Task ReceiveNotification(string message);
}
```

### 🚀 Usage with Generated Interfaces

#### Client-Side Usage

```csharp
// Type-safe handler registration
await manager.RegisterHandler<IChatHubClient, string, string>(
    c => c.ReceiveMessage,
    async (user, msg) => Console.WriteLine($"{user}: {msg}"));

// Type-safe method invocation
await manager.InvokeAsync<IChatHubServer, string, string>(
    s => s.SendMessage,

    ### Type-Safe Hub Example (with Source Generator)

    ```csharp
    // Register handler using generated interface
    manager.RegisterHandler<object, string, string>(
        nameof(IChatHubClient.ReceiveMessage),
        (user, msg) => Console.WriteLine($"[Type-Safe Chat] {user}: {msg}"));

    // Type-safe method invocation
    await manager.InvokeAsync<IChatHubServer>(
        s => s.SendMessage("Alice", "Hello from type-safe client!"));
    ```
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
}
```

### ⚙️ Method Attributes

```csharp
// Use these attributes to specify which methods to include
[ServerMethod]  // Marks hub methods callable by clients (REQUIRED)
[ClientMethod]  // Marks client methods callable by server (REQUIRED)

// Examples:
public class ChatHub : Hub
{
    [ServerMethod]  // This method can be called by clients
    public Task SendMessage(string user, string message) 
        => Clients.All.SendAsync("ReceiveMessage", user, message);
    
    [ClientMethod]  // This method can be called by server on clients
    private Task ReceiveMessage(string user, string message) => Task.CompletedTask;
}
```

### 🎯 Benefits of Hub Generator

#### 1. **Zero Configuration**

*   ✅ Automatically detects ALL `Hub` classes
    
*   ✅ No attributes needed on classes
    
*   ✅ Generates interfaces at compile time
    

#### 2. **Compile-Time Safety**

```csharp
// ✅ Compiles - type-safe
await manager.InvokeAsync<IChatHubServer, string, string>(
    s => s.SendMessage, "user", "message");

// ❌ Compile error - method doesn't exist
await manager.InvokeAsync<IChatHubServer, string, string>(
    s => s.NonExistentMethod, "user", "message"); // ERROR!
```

#### 3. **IntelliSense Support**

*   ✅ Full IntelliSense for hub methods
    
*   ✅ Parameter type checking
    
*   ✅ Method discovery
    

#### 4. **Refactoring Friendly**

*   ✅ Rename hub methods safely
    
*   ✅ Change parameters with compile-time checks
    
*   ✅ Find all references works perfectly
    

### 🔧 Setup Requirements

#### 1. **Add to Your Project**

```xml
<!-- Add the Hub Generator package -->
<PackageReference Include="Shaunebu.SignalRManager.HubGenerator" Version="1.0.0" />
```

#### 2. **Enable in Project File**

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

### 📁 Generated Files Location

Generated interfaces are created in:

```text
/bin/Debug/net8.0/Generated/Shaunebu.SignalRManager.HubGenerator/
  - IChatHubClient.g.cs
  - IChatHubServer.g.cs
  - INotificationHubClient.g.cs
  - INotificationHubServer.g.cs
```

### 🚀 Advanced Usage

#### Multiple Hubs Project

```csharp
// All hubs are automatically processed
public class ChatHub : Hub { /* ... */ }
public class NotificationHub : Hub { /* ... */ }
public class AdminHub : Hub { /* ... */ }
public class AnalyticsHub : Hub { /* ... */ }

// All interfaces are generated automatically:
// IChatHubClient, IChatHubServer
// INotificationHubClient, INotificationHubServer  
// IAdminHubClient, IAdminHubServer
// IAnalyticsHubClient, IAnalyticsHubServer
```

#### Custom Method Filtering

```csharp
public class SecureHub : Hub
{
    [ServerMethod]
    public Task PublicMethod(string data) { /* ... */ }
    
    // This won't be included in the interface (no attribute)
    public Task InternalMethod(string data) { /* ... */ }
    
    [ClientMethod] 
    private Task ClientCallback(string result) => Task.CompletedTask;
}
```

* * *

**The Hub Generator transforms SignalR development from string-based magic to type-safe, compiler-verified code, making your SignalR applications more maintainable and less error-prone!** 🎯
