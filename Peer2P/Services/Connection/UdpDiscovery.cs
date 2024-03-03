using System.Net.Sockets;
using System.Text;
using Peer2P.Library.Console.Messaging;
using Peer2P.Services.Connection.Handlers;

namespace Peer2P.Services.Connection;

internal static class UdpDiscovery
{
    private static readonly UdpClient UdpClient = new(Peer2PSettings.Instance.Network.Broadcast.Port)
    {
        EnableBroadcast = true
    };
    
    private static bool _disposed;

    public static async void SendPeriodicAsync(string message, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("The message for udp discovery cannot be null or empty.", nameof(message));
        
        byte[] data = Encoding.ASCII.GetBytes(message);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpClient.Send(data, Peer2PSettings.Instance.Network.Broadcast);

                Logger.Log($"Sent broadcast discovery: {message}")
                    .Type(LogType.Sent).Protocol(LogProtocol.Udp).Display();

                await Task.Delay(Peer2PSettings.Instance.Timing.UdpDiscoveryInterval, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error sending {nameof(UdpDiscovery)} message: {ex.Message}")
                .Type(LogType.Error).Protocol(LogProtocol.Udp).Display();
        }
    }
    
    public static async void ListenIncomingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await UdpClient.ReceiveAsync(cancellationToken);
                byte[] data = result.Buffer;

                if (data.Length == 0) continue;
                if (Equals(Peer2PSettings.Instance.Network.IpAddress, result.RemoteEndPoint.Address)) continue;

                string message = Encoding.UTF8.GetString(data);
                UdpHandler.Handle(message, result.RemoteEndPoint.Address);
            }
            catch (OperationCanceledException)
            {
                Logger.Log($"Receiving data operation from {nameof(UdpDiscovery)} canceled.")
                    .Type(LogType.Warning).Protocol(LogProtocol.Udp).Display();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in {nameof(UdpDiscovery)} when receiving data: {ex.Message}")
                    .Type(LogType.Error).Protocol(LogProtocol.Udp).Display();
            }
        }
    }

    public static void Dispose()
    {
        Dispose(true);
    }

    private static void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            UdpClient.Dispose();
            
            Logger.Log($"{nameof(UdpDiscovery)} is disposed.")
                .Type(LogType.Warning).Protocol(LogProtocol.Udp).Display();
        }

        _disposed = true;
    }
}