using System.Net.Sockets;
using System.Text;
using Peer2P.Library.Console.Messaging;
using Peer2P.Services.Connection.Handlers;

namespace Peer2P.Services.Connection;

internal static class UdpDiscovery
{
    private static readonly UdpClient UdpClient = new(Peer2PSettings.Instance.Communication.BroadcastPort)
    {
        EnableBroadcast = true
    };
    
    private static bool _disposed;
    
    private static void LogDiscoveryMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Udp).Display();
    }

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

                LogDiscoveryMessage($"Sent broadcast discovery: {message}", LogType.Sent);

                await Task.Delay(Peer2PSettings.Instance.Timing.UdpDiscoveryInterval, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogDiscoveryMessage($"Error sending {nameof(UdpDiscovery)} message: {ex.Message}", LogType.Error);
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
                UdpHandler.Handle(message, result.RemoteEndPoint.Address, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                LogDiscoveryMessage($"Receiving data operation from {nameof(UdpDiscovery)} canceled.", LogType.Warning);
            }
            catch (Exception ex)
            {
                LogDiscoveryMessage($"Error in {nameof(UdpDiscovery)} when receiving data: {ex.Message}", LogType.Error);
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
            
            LogDiscoveryMessage($"{nameof(UdpDiscovery)} is disposed.", LogType.Warning);
        }

        _disposed = true;
    }
}