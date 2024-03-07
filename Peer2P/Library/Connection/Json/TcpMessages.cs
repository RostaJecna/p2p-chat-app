using Newtonsoft.Json;

namespace Peer2P.Library.Connection.Json;

public class TcpMessages
{
    [JsonProperty("status")] public string? Status { get; init; }
    [JsonProperty("messages")] public Dictionary<long, PeerMessage>? Messages { get; init; }
}