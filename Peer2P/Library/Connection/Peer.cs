using System.Net;

namespace Peer2P.Library.Connection;

/// <summary>
///     Represents a peer in the peer-to-peer network with an ID and an associated IP address.
/// </summary>
public record Peer(string Id, IPAddress Address)
{
    /// <summary>
    ///     Gets a string representation of the peer, including its ID and IP address.
    /// </summary>
    /// <returns>A string representing the peer.</returns>
    public override string ToString()
    {
        return $"({Id}) - [{Address}]";
    }
}