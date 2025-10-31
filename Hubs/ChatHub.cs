using Microsoft.AspNetCore.SignalR;
using Shaunebu.SignalR.HubGenerator;

namespace Shaunebu.Common.SignalRManager.Client.Hubs;

public class ChatHub : Hub
{
    [ServerMethod]
    public Task SendMessage(string user, string message)
    {
        // Este método puede ser llamado por los clientes
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    [ClientMethod]
    private Task ReceiveMessage(string user, string message)
    {
        return Task.CompletedTask;
    }

    [ServerMethod]
    public Task JoinGroup(string groupName)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    [ClientMethod]
    private Task UserJoined(string user) => Task.CompletedTask;
}

