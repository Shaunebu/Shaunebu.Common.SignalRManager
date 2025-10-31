using Shaunebu.Common.SignalRManager.Abstractions;

namespace Shaunebu.Common.SignalRManager.Client.Clients;

public class ChatHubClient
{
    private readonly ISignalRManager _manager;

    public ChatHubClient(ISignalRManager manager)
    {
        _manager = manager;
    }

    // Server methods (client calls server)
    public Task SendMessageAsync(string user, string message)
    {
        return _manager.InvokeAsync<object>("SendMessage", new object[] { user, message });
    }

    public Task JoinGroupAsync(string groupName)
    {
        return _manager.InvokeAsync<object>("JoinGroup", new object[] { groupName });
    }

    public Task LeaveGroupAsync(string groupName)
    {
        return _manager.InvokeAsync<object>("LeaveGroup", new object[] { groupName });
    }

    // Client methods (server calls client)
    public IDisposable OnMessageReceived(Action<string, string> handler)
    {
        // For multiple parameters, we need to handle them differently
        // Since RegisterHandler<T> only takes one type, we'll use a wrapper
        return _manager.RegisterHandler<MessageWrapper>("ReceiveMessage", wrapper =>
            handler(wrapper.User, wrapper.Message));
    }

    public IDisposable OnMessageReceivedAsync(Func<string, string, Task> handler)
    {
        return _manager.RegisterAsyncHandler<MessageWrapper>("ReceiveMessage", async wrapper =>
            await handler(wrapper.User, wrapper.Message));
    }

    public IDisposable OnUserJoined(Action<string> handler)
    {
        return _manager.RegisterHandler<string>("UserJoined", handler);
    }

    public IDisposable OnUserJoinedAsync(Func<string, Task> handler)
    {
        return _manager.RegisterAsyncHandler<string>("UserJoined", handler);
    }

    // Helper class for multiple parameters
    private class MessageWrapper
    {
        public string User { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}