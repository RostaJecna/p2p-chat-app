using System.Net;
using System.Net.Sockets;

namespace Peer2P.Library.Extensions;

internal static class TcpClientExtensions
{
    public static IPEndPoint? GetIpv4EndPoint(this TcpClient? client)
    {
        IPEndPoint? remoteIpEndPoint = (IPEndPoint?)client?.Client.RemoteEndPoint;
        return remoteIpEndPoint is not null
            ? new IPEndPoint(remoteIpEndPoint.Address.MapToIPv4(), remoteIpEndPoint.Port)
            : null;
    }
    
    public static bool IsSameIpv4Address(this TcpClient? tcpClient, IPAddress ipAddress)
    {
        IPEndPoint? remoteIpEndPoint = tcpClient.GetIpv4EndPoint();
        return remoteIpEndPoint is not null && remoteIpEndPoint.Address.Equals(ipAddress);
    }
    
    public static bool IsStillConnected(this TcpClient? tcpClient)
    {
        return tcpClient is { Connected: true };
    }
}