using Microsoft.AspNetCore.SignalR;
using SignalRHub.Services;

namespace SignalRHub;

public class MessageHub : Hub
{
    private readonly FilterService _filterService;

    public MessageHub(FilterService filterService)
    {
        _filterService = filterService;
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task Subscribe(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return;

        var oldGroup = _filterService.GetConnectionGroup(Context.ConnectionId);
        if (oldGroup != null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldGroup);

        await Groups.AddToGroupAsync(Context.ConnectionId, filter);
        _filterService.TrackSubscription(Context.ConnectionId, oldGroup, filter);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = _filterService.GetConnectionGroup(Context.ConnectionId);
        if (group != null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

        _filterService.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
