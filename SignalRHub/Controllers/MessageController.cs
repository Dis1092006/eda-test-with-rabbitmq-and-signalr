using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Services;

namespace SignalRHub.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly FilterService _filterService;

    public MessageController(IHubContext<MessageHub> hubContext, FilterService filterService)
    {
        _hubContext = hubContext;
        _filterService = filterService;
    }

    [HttpPost]
    [Route("/message")]
    public async Task<IActionResult> Post([FromBody] string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", message);

        var matchingConnections = _filterService.GetMatchingConnections(message).ToList();
        if (matchingConnections.Count > 0)
        {
            await _hubContext.Clients.Clients(matchingConnections)
                .SendAsync("ReceiveFilteredMessage", "Server", message);
        }

        return Ok();
    }
}