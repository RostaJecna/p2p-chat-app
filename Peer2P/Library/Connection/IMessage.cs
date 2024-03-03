using Newtonsoft.Json;

namespace Peer2P.Library.Connection;

internal interface IMessage
{
    [JsonProperty("peer_id")] string? PeerId { get; init; }
}