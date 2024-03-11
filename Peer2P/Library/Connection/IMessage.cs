using Newtonsoft.Json;

namespace Peer2P.Library.Connection;

/// <summary>
///     Represents the interface for messages in the peer-to-peer network.
/// </summary>
internal interface IMessage
{
    /// <summary>
    ///     Gets or sets the peer ID associated with the message.
    /// </summary>
    /// <remarks>
    ///     The peer ID is a unique identifier representing the sender or recipient of the message.
    /// </remarks>
    [JsonProperty("peer_id")]
    string? PeerId { get; init; }
}