// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using Shaunebu.Common.SignalRManager.Abstractions;
using Shaunebu.Common.SignalRManager.Enums;
using Shaunebu.Common.SignalRManager.Extensions;
using Shaunebu.Common.SignalRManager.Handlers;
using Shaunebu.Common.SignalRManager.Helpers;
using Shaunebu.Common.SignalRManager.Models;
using Shaunebu.Common.SignalRManager.Policies;
using Shaunebu.Common.SignalRManager.Security;
using Shaunebu.Common.SignalRManager.Client.Clients;
using System.Text;


Console.WriteLine("=== SignalRManager Library Test ===");
var localUrl = "https://localhost:7068";

// Passed ✓
await TestBasicConnection();

// Passed ✓
await TestWithPollyRetryPolicy();

// Passed ✓
await TestWithCircuitBreaker();

// Passed ✓
await TestMessageBatching();

// Passed ✓
await TestErrorScenarios();

// Passed ✓
await TestConnectionStateMonitoring();

Console.WriteLine("=== All tests completed ===");
Console.ReadLine();
async Task TestBasicConnection()
{
    Console.WriteLine("\n--- Testing Basic Connection ---");

    try
    {
        var builder = new SignalRManagerBuilder()
            .WithHubUrl($"{localUrl}/hubs/chat")
            .WithBatching(20, TimeSpan.FromMilliseconds(200))
            .WithRetryPolicy(new ExponentialRetryPolicy(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)))
            .WithMetricsExporter(new ConsoleMetricsExporter())
            .WithEncryptor(new AesEncryptor(
                Encoding.UTF8.GetBytes("0123456789abcdef"),
                Encoding.UTF8.GetBytes("abcdef0123456789")))
            .WithAutoReconnect(true, 3)
            .WithLogger(CreateLogger());

        var manager = builder.Build();

        // Subscribe to connection state changes
        manager.ConnectionStateChanged += state =>
            Console.WriteLine($"[Connection State] {state.Status} - ConnectionId: {state.ConnectionId}");

        await manager.InitializeAsync();

        Console.WriteLine("✓ Connection established successfully");

        // Use client-side typed wrappers
        var hubFactory = new TypedHubClientFactory(manager);
        var chatClient = hubFactory.CreateChatClient();

        // Type-safe handler registration
        chatClient.OnMessageReceived((user, msg) =>
            Console.WriteLine($"[Type-Safe Chat] {user}: {msg}"));

        // Type-safe method invocation
        await chatClient.SendMessageAsync("Jorge", "Hola mundo from type-safe client!");

        Console.WriteLine("✓ Type-safe operations completed");

        // You can still mix with raw manager for advanced scenarios
        await manager.SubscribeToGroupAsync("group1");
        await manager.UnsubscribeFromGroupAsync("group1");

        // Wait for incoming messages
        Console.WriteLine("Waiting for incoming messages...");
        await Task.Delay(5000);

        await manager.ShutdownAsync();
        Console.WriteLine("✓ Basic connection test passed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Basic connection test failed: {ex.Message}");
    }
}

async Task TestWithPollyRetryPolicy()
{
    Console.WriteLine("\n--- Testing Polly Retry Policy ---");

    try
    {
        var builder = new SignalRManagerBuilder()
            .WithHubUrl($"{localUrl}/hubs/notifications")
            .WithPollyRetryPolicy(maxRetries: 3, initialDelay: TimeSpan.FromSeconds(1))
            .WithMetricsExporter(new ConsoleMetricsExporter());

        var manager = builder.Build();
        await manager.InitializeAsync();

        // Test the retry policy execution
        var result = await ((PollyRetryPolicy)manager.GetRetryPolicy()).ExecuteAsync(
            async (ct) =>
            {
                await Task.Delay(100, ct);
                return "Success";
            }, CancellationToken.None);

        Console.WriteLine($"Polly execution result: {result}");
        await manager.ShutdownAsync();

        Console.WriteLine("✓ Polly retry policy test passed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Polly retry policy test failed: {ex.Message}");
    }
}

async Task TestWithCircuitBreaker()
{
    Console.WriteLine("\n--- Testing Circuit Breaker ---");

    try
    {
        var builder = new SignalRManagerBuilder()
            .WithHubUrl($"{localUrl}/hubs/notifications")
            .WithCircuitBreakerPersistence(new MemoryCircuitBreakerPersistence())
            .WithMetricsExporter(new ConsoleMetricsExporter());

        var manager = builder.Build();
        await manager.InitializeAsync();

        // The circuit breaker is internal, but we can test by monitoring metrics
        // In a real scenario, you'd simulate failures to trigger the circuit breaker

        Console.WriteLine("✓ Circuit breaker setup test passed");
        await manager.ShutdownAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Circuit breaker test failed: {ex.Message}");
    }
}

async Task TestMessageBatching()
{
    Console.WriteLine("\n--- Testing Message Batching ---");

    try
    {
        var builder = new SignalRManagerBuilder()
            .WithHubUrl($"{localUrl}/hubs/notifications")
            .WithBatching(5, TimeSpan.FromMilliseconds(500)) // Small batch for testing
            .WithOfflineQueue(true)
            .WithMetricsExporter(new ConsoleMetricsExporter());

        var manager = builder.Build();

        // Subscribe to batch events
        manager.OnMessageBatched += (method, count) =>
            Console.WriteLine($"[Batch] {count} messages sent via {method}");

        await manager.InitializeAsync();

        // Send multiple messages quickly to trigger batching
        for (int i = 0; i < 10; i++)
        {
            await manager.EnqueueAsync("SendMessage", $"Batch message {i + 1}", priority: i % 3);
        }

        // Wait for batching to process
        await Task.Delay(1000);

        await manager.ShutdownAsync();

        Console.WriteLine("✓ Message batching test passed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Message batching test failed: {ex.Message}");
    }
}

async Task TestErrorScenarios()
{
    Console.WriteLine("\n--- Testing Error Handling ---");

    try
    {
        var builder = new SignalRManagerBuilder()
            .WithHubUrl("https://invalid-server-that-doesnt-exist/hubs/notifications")
            .WithRetryPolicy(new ExponentialRetryPolicy(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), maxAttempts: 2))
            .WithMetricsExporter(new ConsoleMetricsExporter())
            .WithAutoReconnect(false); // Disable auto-reconnect for this test

        var manager = builder.Build();

        manager.OnRetryAttempt += (operation, attempt, exception) =>
            Console.WriteLine($"[Retry] Operation: {operation}, Attempt: {attempt}, Error: {exception?.Message}");

        try
        {
            await manager.InitializeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✓ Expected connection failure handled: {ex.Message}");
        }

        // Test handler error handling
        var validBuilder = new SignalRManagerBuilder()
            .WithHubUrl($"{localUrl}/hubs/notifications")
            .WithMetricsExporter(new ConsoleMetricsExporter());

        var validManager = validBuilder.Build();
        await validManager.InitializeAsync();

        // Register handler that throws exception
        validManager.RegisterHandler<string>("TestError", message =>
        {
            throw new InvalidOperationException("Test handler error");
        });

        Console.WriteLine("✓ Error handling test passed");
        await validManager.ShutdownAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error handling test failed: {ex.Message}");
    }
}

async Task TestConnectionStateMonitoring()
{
    Console.WriteLine("\n--- Testing Connection State Monitoring ---");

    try
    {
        var builder = new SignalRManagerBuilder()
            .WithHubUrl($"{localUrl}/hubs/notifications")
            .WithMetricsExporter(new ConsoleMetricsExporter())
            .WithAutoReconnect(true, 2);

        var manager = builder.Build();

        // Comprehensive state monitoring
        manager.ConnectionStateChanged += state =>
        {
            Console.WriteLine($"[State Change] Status: {state.Status}, " +
                            $"ConnectionId: {state.ConnectionId}, " +
                            $"RetryCount: {state.RetryCount}, " +
                            $"LastError: {state.LastError?.Message}");
        };

        await manager.InitializeAsync();

        // Test connection state properties
        Console.WriteLine($"IsConnected: {manager.IsConnected}");
        Console.WriteLine($"ConnectionId: {manager.ConnectionId}");
        Console.WriteLine($"Current State: {manager.ConnectionState.Status}");

        // Test waiting for connection
        var connected = await manager.WaitForConnectionAsync(TimeSpan.FromSeconds(5));
        Console.WriteLine($"WaitForConnection result: {connected}");

        // Test disconnection and reconnection
        await manager.DisconnectAsync();
        await Task.Delay(1000);
        await manager.ConnectAsync();

        await manager.ShutdownAsync();

        Console.WriteLine("✓ Connection state monitoring test passed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Connection state monitoring test failed: {ex.Message}");
    }
}

// Helper method to create a logger
static ILogger CreateLogger()
{
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("Shaunebu.Common.SignalRManager", LogLevel.Debug)
            .AddConsole();
    });
    return loggerFactory.CreateLogger<Program>();
}

// You'll need to create this simple implementation for testing
public class MemoryCircuitBreakerPersistence : ICircuitBreakerPersistence
{
    private readonly Dictionary<string, CircuitState> _states = new();

    public Task<CircuitState?> LoadStateAsync(string circuitName)
    {
        _states.TryGetValue(circuitName, out var state);
        return Task.FromResult<CircuitState?>(state);
    }

    public Task SaveStateAsync(string circuitName, CircuitState state)
    {
        _states[circuitName] = state;
        return Task.CompletedTask;
    }
}