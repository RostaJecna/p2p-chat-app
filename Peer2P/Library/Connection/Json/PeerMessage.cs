using Newtonsoft.Json;

namespace Peer2P.Library.Connection.Json;

public record PeerMessage : IMessage
{
    [JsonProperty("peer_id")] public string? PeerId { get; init; }
    [JsonProperty("message")] public string? Message { get; set; }
}