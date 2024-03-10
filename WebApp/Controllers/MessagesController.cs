using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Peer2P.Library.Console.Messaging;
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
            Logger.Log($"Received GET request on /api/send with message: {message}")
                .Type(LogType.Received).Protocol(LogProtocol.Http).Display();
            
            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            TcpConnections.BroadcastToClients(NetworkData.SerializeNewMessage(milliseconds, message));

            return Ok($"Server received GET request with message: {message}");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error handling GET request on /api/send: {ex.Message}")
                .Type(LogType.Error).Protocol(LogProtocol.Http).Display();
            return StatusCode(500, $"Error handling request: {ex.Message}");
        }
    }
    
    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        try
        {
            Logger.Log("Received GET request on /api/messages")
                .Type(LogType.Received).Protocol(LogProtocol.Http).Display();
            
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
            Logger.Log($"Error handling GET request on /api/messages: {ex.Message}")
                .Type(LogType.Error).Protocol(LogProtocol.Http).Display();
            return StatusCode(500, $"Error handling request: {ex.Message}");
        }
    }
}