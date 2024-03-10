using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;
using Peer2P.Library.Console.Messaging;
using Peer2P.Library.Extensions;
using Peer2P.Services.Connection.Handlers;

namespace Peer2P.Services.Connection;

public static class TcpConnections
{
    private static readonly HashSet<TcpClient> ConnectedClients = new();

    private static void LogTcpMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Tcp).Display();
    }

    public static bool HasConnectionWith(IPAddress? ipAddress)
    {
        return ipAddress is not null &&
               ConnectedClients.Any(client => client.IsStillConnected() && client.IsSameIpv4Address(ipAddress));
    }
    
    private static string GetTargetEndPointDescription(IPEndPoint? ipEndPoint)
    {
        string? peerId = UdpHandler.GetPeerIdByAddress(ipEndPoint?.Address);
        return $"({peerId ?? "Unknown"}) - [{ipEndPoint}]";
    }

    public static void BroadcastToClients(string message)
    {
        foreach (TcpClient client in ConnectedClients)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                if(!stream.CanWrite) continue;
                
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(data);

                IPEndPoint? ipEndPoint = (IPEndPoint?)client.Client.RemoteEndPoint;
                string target = GetTargetEndPointDescription(ipEndPoint);
                
                LogTcpMessage($"Sent message to connected client {target}: {message}", LogType.Sent);
            }
            catch (Exception ex)
            {
                LogTcpMessage($"Error sending message to connected client: {ex.Message}", LogType.Error);
            }
        }
    }

    private static void LogCurrentConnections()
    {
        try
        {
            if (ConnectedClients.Count == 0) return;

            string clients = string.Join(", ", ConnectedClients.Select(client =>
            {
                IPEndPoint? remoteIpEndPoint = (IPEndPoint?)client.Client.RemoteEndPoint;
                return remoteIpEndPoint != null
                    ? new IPEndPoint(remoteIpEndPoint.Address.MapToIPv4(), remoteIpEndPoint.Port)
                    : null;
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
                if (ConnectedClients.Count == 0)
                {
                    await Task.Delay(timeout, cancellationToken);
                    continue;
                }

                HashSet<TcpClient?> clientsToRemove = new();

                foreach (TcpClient client in ConnectedClients)
                    try
                    {
                        if(client.IsStillConnected()) continue;
                        
                        IPEndPoint? ipv4EndPoint = client.GetIpv4EndPoint();
                        
                        if (ipv4EndPoint is null)
                        {
                            clientsToRemove.Add(client);
                            continue;
                        }

                        if (!uniqueIpAddresses.Add(ipv4EndPoint.Address))
                        {
                            LogTcpMessage($"Duplicate IP address {ipv4EndPoint} found: Removing the client...", LogType.Warning);
                            clientsToRemove.Add(client);
                            continue;
                        }

                        string target = GetTargetEndPointDescription(ipv4EndPoint);

                        LogTcpMessage($"Lost connection with {target}: Removing the client...", LogType.Warning);
                        clientsToRemove.Add(client);
                    }
                    catch (NullReferenceException)
                    {
                        LogTcpMessage("Lost reference to client while checking connection: Removing...", LogType.Error);
                        clientsToRemove.Add(client);
                    }
                    catch (Exception ex)
                    {
                        LogTcpMessage($"Error handling lost connection: {ex.Message}", LogType.Error);
                        clientsToRemove.Add(client);
                    }

                foreach (TcpClient client in clientsToRemove.OfType<TcpClient>())
                {
                    ConnectedClients.Remove(client);
                    client.Dispose();
                }

                LogCurrentConnections();

                if (uniqueIpAddresses.Count > 0) uniqueIpAddresses.Clear();
                await Task.Delay(timeout, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogTcpMessage($"Error while checking connected clients: {ex.Message}", LogType.Error);
        }
    }

    private static async Task ListenForNewMessages(Stream stream, Peer peer,
        CancellationToken cancellationToken)
    {
        stream.ReadTimeout = Peer2PSettings.Instance.Timing.ClientTimeoutDelay * 2;

        byte[] buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            LogTcpMessage($"Received data from connected client {peer} - {bytesRead} bytes: Decoding...",
                LogType.Received);
            string message = Encoding.UTF8.GetString(buffer);
            message = message.Replace("\r", "").Replace("\n", "");
            
            if (string.IsNullOrWhiteSpace(message))
            {
                LogTcpMessage($"Decoded message from connected client is empty {peer}: Ignoring...",
                    LogType.Warning);
                continue;
            }

            LogTcpMessage($"Successfully decoded message from {peer}: {message}", LogType.Successful);

            JObject jMessage = JObject.Parse(message);

            if (jMessage["command"] != null)
            {
                NewMessage? newMessage = jMessage.ToObject<NewMessage>();

                if (newMessage == null ||
                    newMessage.Command != Peer2PSettings.Instance.Communication.Commands.OnNewMessage)
                {
                    LogTcpMessage("Received invalid new message from connected client: Ignoring...", LogType.Warning);
                    continue;
                }
                
                NetworkData.AddMessage(newMessage, peer);

                LogTcpMessage(
                    $"Decoded message from connected client {peer}: [{newMessage.MessageId} - {newMessage.Message}]",
                    LogType.Received);

                byte[] data = Encoding.UTF8.GetBytes(NetworkData.ReqResPair.EmptyStatus + "\n");
                await stream.WriteAsync(data, cancellationToken);

                LogTcpMessage($"Sent response on new message to {peer}: {NetworkData.ReqResPair.EmptyStatus}",
                    LogType.Sent);
                continue;
            }
            
            if(jMessage["status"]?.Value<string>() == Peer2PSettings.Instance.Communication.Status.OnResponse)
            {
                LogTcpMessage($"Received status on sent new message to connected client {peer}: {message}",
                    LogType.Received);
            }
            else
            {
                LogTcpMessage($"Received unknown message type from connected client {peer}: {message}", LogType.Expecting);
            }
        }
    }

    public static async void StoreClientAsync(TcpClient client, NetworkStream stream, Peer peer,
        CancellationToken cancellationToken)
    {
        try
        {
            ConnectedClients.Add(client);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"Error storing client: {ex.Message}", LogType.Error);
        }

        try
        {
            await ListenForNewMessages(stream, peer, cancellationToken);
            LogTcpMessage($"Timeout waiting new messages from connected client {peer}", LogType.Error);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"Error listening {peer} for new messages: {ex.Message}", LogType.Error);
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }
}