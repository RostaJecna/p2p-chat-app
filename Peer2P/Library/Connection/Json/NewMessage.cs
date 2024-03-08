using Newtonsoft.Json;

namespace Peer2P.Library.Connection.Json;

public record NewMessage
{
    [JsonProperty("command")] public string? Command { get; init; }
    [JsonProperty("message_id")] public long MessageId { get; init; } 
    [JsonProperty("message")] public string? Message { get; init; }
}