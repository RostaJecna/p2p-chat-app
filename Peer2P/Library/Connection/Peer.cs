using System.Net;

namespace Peer2P.Library.Connection;

public record Peer(string Id, IPAddress Address)
{
    public override string ToString()
    {
        return $"({Id}) - [{Address}]";
    }
}