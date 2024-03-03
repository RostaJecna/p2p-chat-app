using Newtonsoft.Json;

namespace Peer2P.Library.Connection.Json;

public record CommandMessage : IMessage
{
    [JsonProperty("command")] public string? Command { get; init; }
    [JsonProperty("peer_id")] public string? PeerId { get; init; }
}