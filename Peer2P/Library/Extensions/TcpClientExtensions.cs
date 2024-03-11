using System.Net;
using System.Net.Sockets;

namespace Peer2P.Library.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="TcpClient" /> class.
/// </summary>
internal static class TcpClientExtensions
{
    /// <summary>
    ///     Gets the IPv4 endpoint of the remote client associated with the <see cref="TcpClient" />.
    /// </summary>
    /// <param name="client">The <see cref="TcpClient" /> instance.</param>
    /// <returns>The IPv4 endpoint of the remote client, or null if not applicable.</returns>
    public static IPEndPoint? GetIpv4EndPoint(this TcpClient? client)
    {
        IPEndPoint? remoteIpEndPoint = (IPEndPoint?)client?.Client.RemoteEndPoint;
        return remoteIpEndPoint is not null
            ? new IPEndPoint(remoteIpEndPoint.Address.MapToIPv4(), remoteIpEndPoint.Port)
            : null;
    }

    /// <summary>
    ///     Checks if the remote client associated with the <see cref="TcpClient" /> has the same IPv4 address as the specified
    ///     <paramref name="ipAddress" />.
    /// </summary>
    /// <param name="tcpClient">The <see cref="TcpClient" /> instance.</param>
    /// <param name="ipAddress">The IPv4 address to compare.</param>
    /// <returns>True if the remote client has the same IPv4 address; otherwise, false.</returns>
    public static bool IsSameIpv4Address(this TcpClient? tcpClient, IPAddress ipAddress)
    {
        IPEndPoint? remoteIpEndPoint = tcpClient.GetIpv4EndPoint();
        return remoteIpEndPoint is not null && remoteIpEndPoint.Address.Equals(ipAddress);
    }

    /// <summary>
    ///     Checks if the <see cref="TcpClient" /> is still connected.
    /// </summary>
    /// <param name="tcpClient">The <see cref="TcpClient" /> instance.</param>
    /// <returns>True if the <see cref="TcpClient" /> is still connected; otherwise, false.</returns>
    public static bool IsStillConnected(this TcpClient? tcpClient)
    {
        return tcpClient is { Connected: true };
    }
}