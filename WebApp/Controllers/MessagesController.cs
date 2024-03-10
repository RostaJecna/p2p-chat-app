using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Peer2P.Services.Connection;

namespace WebApp.Controllers;

[Route("api")]
[ApiController]
public class MessagesController : ControllerBase
{
    [HttpGet("send")]
    public IActionResult Send([FromQuery] string message)
    {
        try
        {
            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            TcpConnections.BroadcastToClients(NetworkData.SerializeNewMessage(milliseconds, message));

            return Ok($"Received GET request on /send with message: {message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error handling request: {ex.Message}");
        }
    }
    
    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        try
        {
            string jsonMessages = NetworkData.SerializeAllMessages(false, Formatting.Indented);
            
            return new ContentResult
            {
                Content = jsonMessages,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error handling request: {ex.Message}");
        }
    }
}