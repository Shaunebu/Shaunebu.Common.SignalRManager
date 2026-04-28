using Microsoft.AspNetCore.SignalR;
using Shaunebu.SignalR.HubGenerator;

namespace Shaunebu.Common.SignalRManager.Client.Hubs;

public class NotificationHub : Hub
{
    [ServerMethod]
    public Task SendNotification(string message)
        => Clients.All.SendAsync("ReceiveNotification", message);

    [ClientMethod]
    private Task ReceiveNotification(string message) => Task.CompletedTask;
}
