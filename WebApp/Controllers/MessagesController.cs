using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Peer2P.Library.Console.Messaging;
using Peer2P.Services.Connection;

namespace WebApp.Controllers;

/// <summary>
///     Controller responsible for handling HTTP API requests related to messages.
/// </summary>
[Route("api")]
[ApiController]
public class MessagesController : ControllerBase
{
    /// <summary>
    ///     Sends a new message to all connected peers.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>OK status if the message is sent successfully, or an error status otherwise.</returns>
    [HttpGet("send")]
    public IActionResult Send([FromQuery] string message)
    {
        try
        {
            Logger.Log($"Received GET request on /api/send with message: {message}")
                .Type(LogType.Received).Protocol(LogProtocol.Http).Display();

            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string jsonMessage = NetworkData.SerializeNewMessage(milliseconds, message);
            NetworkData.AddMessage(milliseconds, message);
            TcpConnections.BroadcastToClients(jsonMessage);

            return Ok($"Server received GET request with message: {message}");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error handling GET request on /api/send: {ex.Message}")
                .Type(LogType.Error).Protocol(LogProtocol.Http).Display();
            return StatusCode(500, $"Error handling request: {ex.Message}");
        }
    }

    /// <summary>
    ///     Retrieves all stored messages.
    /// </summary>
    /// <returns>
    ///     JSON-formatted content containing all stored messages,
    ///     or an error status if there's an issue handling the request.
    /// </returns>
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