using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace SignalRHub.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IHubContext<MessageHub> _hubContext;

    public MessageController(IHubContext<MessageHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    [HttpPost]
    [Route("/message")]
    public async Task<IActionResult> Post([FromBody] string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", message);
        return Ok();
    }
}