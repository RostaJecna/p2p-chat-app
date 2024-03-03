using Newtonsoft.Json;

namespace Peer2P.Library.Connection;

public interface IMessage
{
    [JsonProperty("peer_id")] string? PeerId { get; init; }
}