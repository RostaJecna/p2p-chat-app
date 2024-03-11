using System.Net;
using System.Net.Sockets;
using System.Text;
using Peer2P.Library.Console.Messaging;
using Peer2P.Services.Connection.Handlers;

namespace Peer2P.Services.Connection;

/// <summary>
///     Manages UDP discovery operations for peer-to-peer communication.
/// </summary>
internal static class UdpDiscovery
{
    private static readonly UdpClient UdpClient = new(Peer2PSettings.Instance.Communication.BroadcastPort)
    {
        EnableBroadcast = true
    };
    
    /// <summary>
    ///     Logs a discovery message with the specified content and log type.
    /// </summary>
    /// <param name="message">The content of the log message.</param>
    /// <param name="type">The log type associated with the message.</param>
    private static void LogDiscoveryMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Udp).Display();
    }

    /// <summary>
    ///     Sends a UDP message to the specified remote endpoint.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="remote">The remote endpoint to send the message to.</param>
    /// <param name="encoding">The encoding used for the message.</param>
    public static void SendTo(string message, IPEndPoint remote, Encoding encoding)
    {
        byte[] data = encoding.GetBytes(message);
        UdpClient.Send(data, remote);
    }

    /// <summary>
    ///     Sends periodic UDP discovery messages asynchronously.
    /// </summary>
    /// <param name="message">The discovery message to send.</param>
    /// <param name="cancellationToken">The cancellation token to stop the periodic sending.</param>
    public static async void SendPeriodicAsync(string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
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

    /// <summary>
    ///     Listens for incoming UDP discovery messages asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop listening.</param>
    public static async void ListenIncomingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
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
                LogDiscoveryMessage($"Error in {nameof(UdpDiscovery)} when receiving data: {ex.Message}",
                    LogType.Error);
            }
    }
}