using Shaunebu.Common.SignalRManager.Abstractions;

namespace Shaunebu.Common.SignalRManager.Client.Clients;

public class TypedHubClientFactory
{
    private readonly ISignalRManager _manager;

    public TypedHubClientFactory(ISignalRManager manager)
    {
        _manager = manager;
    }

    public ChatHubClient CreateChatClient()
    {
        return new ChatHubClient(_manager);
    }

    public NotificationHubClient CreateNotificationClient()
    {
        return new NotificationHubClient(_manager);
    }
}