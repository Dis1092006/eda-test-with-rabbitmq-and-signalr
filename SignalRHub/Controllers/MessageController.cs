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

        foreach (var group in _filterService.GetMatchingGroups(message))
        {
            await _hubContext.Clients.Group(group)
                .SendAsync("ReceiveFilteredMessage", "Server", message);
        }

        return Ok();
    }
}
