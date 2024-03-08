using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Peer2P.Library.Console.Messaging;
using Peer2P.Library.Extensions;
using Peer2P.Services.Connection.Handlers;

namespace Peer2P.Services.Connection;

internal static class TcpConnections
{
    private static readonly ConcurrentBag<TcpClient> ConnectedClients = new();

    private static void LogTcpMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Tcp).Display();
    }

    public static bool HasConnectionWith(IPAddress? ipAddress)
    {
        return ipAddress is not null &&
               ConnectedClients.Any(client => client.IsStillConnected() && client.IsSameIpv4Address(ipAddress));
    }

    private static void LogCurrentConnections()
    {
        try
        {
            if (ConnectedClients.IsEmpty) return;

            string clients = string.Join(", ", ConnectedClients.Select(client =>
            {
                IPEndPoint? endPoint = client.GetIpv4EndPoint();
                string? peerId = endPoint != null ? UdpHandler.GetPeerIdByAddress(endPoint.Address) : null;
                return $"({peerId ?? "Unknown"}) - [{endPoint ?? new IPEndPoint(IPAddress.None, 0)}]";
            }));

            LogTcpMessage($"Current connections with ({ConnectedClients.Count}) - {clients}", LogType.Expecting);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"Error logging current connections: {ex.Message}", LogType.Error);
        }
    }

    public static async void StartCheckConnectedClientsAsync(CancellationToken cancellationToken)
    {
        int timeout = Peer2PSettings.Instance.Timing.ClientTimeoutDelay;
        HashSet<IPAddress> uniqueIpAddresses = new();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (ConnectedClients.IsEmpty)
                {
                    await Task.Delay(timeout, cancellationToken);
                    continue;
                }

                List<TcpClient> clientsToRemove = new();

                foreach (TcpClient client in ConnectedClients)
                    try
                    {
                        IPEndPoint? keyEndPoint = client.GetIpv4EndPoint();

                        if (keyEndPoint is not null)
                        {
                            string? peerId = UdpHandler.GetPeerIdByAddress(keyEndPoint.Address);
                            string target = $"({peerId ?? "Unknown"}) - [{keyEndPoint}]";

                            if (!uniqueIpAddresses.Add(keyEndPoint.Address))
                            {
                                LogTcpMessage($"Duplicate IP address detected: {target}. Removing the client...",
                                    LogType.Warning);
                                clientsToRemove.Add(client);
                                continue;
                            }

                            if (client.IsStillConnected()) continue;

                            LogTcpMessage($"Lost connection with {target}: Removing the client...", LogType.Warning);
                            clientsToRemove.Add(client);
                        }
                        else
                        {
                            clientsToRemove.Add(client);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTcpMessage($"Error handling lost connection: {ex.Message}", LogType.Error);
                    }
                
                LogCurrentConnections();

                foreach (TcpClient client in clientsToRemove)
                {
                    ConnectedClients.TryTake(out _);
                    client.Dispose();
                }

                if (uniqueIpAddresses.Count > 0) uniqueIpAddresses.Clear();
                await Task.Delay(timeout, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogTcpMessage($"Error while checking connected clients: {ex.Message}", LogType.Error);
        }
    }


    public static void StoreClient(TcpClient client, NetworkStream stream)
    {
        ConnectedClients.Add(client);
        // throw new NotImplementedException();
    }
}