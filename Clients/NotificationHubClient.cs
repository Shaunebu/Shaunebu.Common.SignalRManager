using Shaunebu.Common.SignalRManager.Abstractions;

namespace Shaunebu.Common.SignalRManager.Client.Clients;

public class NotificationHubClient
{
    private readonly ISignalRManager _manager;

    public NotificationHubClient(ISignalRManager manager)
    {
        _manager = manager;
    }

    // Server methods
    public Task SendNotificationAsync(string message)
    {
        return _manager.InvokeAsync<object>("SendNotification", new object[] { message });
    }

    // Client methods
    public IDisposable OnNotificationReceived(Action<string> handler)
    {
        return _manager.RegisterHandler<string>("ReceiveNotification", handler);
    }

    public IDisposable OnNotificationReceivedAsync(Func<string, Task> handler)
    {
        return _manager.RegisterAsyncHandler<string>("ReceiveNotification", handler);
    }
}