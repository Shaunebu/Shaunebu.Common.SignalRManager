using Microsoft.AspNetCore.SignalR;
using Shaunebu.SignalR.HubGenerator;

namespace Shaunebu.Common.SignalRManager.Client.Hubs;

public class ChatHub : Hub
{
    [ServerMethod]
    public Task SendMessage(string user, string message)
        => Clients.All.SendAsync("ReceiveMessage", user, message);

    [ServerMethod]
    public Task JoinGroup(string groupName)
        => Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    [ClientMethod]
    private Task ReceiveMessage(string user, string message) => Task.CompletedTask;

    [ClientMethod]
    private Task UserJoined(string user) => Task.CompletedTask;

    [ServerMethod]
    public Task LeaveGroup(string groupName)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
}