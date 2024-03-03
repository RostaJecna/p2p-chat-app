using Newtonsoft.Json;

namespace Peer2P.Library.Connection.Json;

internal record StatusMessage : IMessage
{
    [JsonProperty("status")] public string? Status { get; init; }
    [JsonProperty("peer_id")] public string? PeerId { get; init; }
}