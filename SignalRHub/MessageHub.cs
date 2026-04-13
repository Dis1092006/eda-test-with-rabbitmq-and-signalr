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

    public Task Subscribe(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return Task.CompletedTask;
        _filterService.SetFilter(Context.ConnectionId, filter);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _filterService.RemoveFilter(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}