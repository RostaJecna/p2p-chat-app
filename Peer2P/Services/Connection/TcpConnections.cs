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

/// <summary>
///     Manages TCP connections, communication, and monitoring with clients.
/// </summary>
public static class TcpConnections
{
    /// <summary>
    ///     Represents a collection of connected TCP clients.
    /// </summary>
    private static readonly HashSet<TcpClient> ConnectedClients = new();

    /// <summary>
    ///     Logs TCP-related messages with specified message and log type.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="type">The log type.</param>
    private static void LogTcpMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Tcp).Display();
    }

    /// <summary>
    ///     Checks if there is an active TCP connection with the specified IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to check for an active connection.</param>
    /// <returns>True if there is an active connection, otherwise false.</returns>
    public static bool HasConnectionWith(IPAddress? ipAddress)
    {
        return ipAddress is not null &&
               ConnectedClients.Any(client => client.IsStillConnected() && client.IsSameIpv4Address(ipAddress));
    }

    /// <summary>
    ///     Gets a human-readable description of the target endpoint using the provided IP endpoint.
    /// </summary>
    /// <param name="ipEndPoint">The IP endpoint to describe.</param>
    /// <returns>A formatted string describing the target endpoint.</returns>
    private static string GetTargetEndPointDescription(IPEndPoint? ipEndPoint)
    {
        string? peerId = UdpHandler.GetPeerIdByAddress(ipEndPoint?.Address);
        return $"({peerId ?? "Unknown"}) - [{ipEndPoint}]";
    }

    /// <summary>
    ///     Broadcasts a message to all connected clients synchronously.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <remarks>
    ///     This method iterates through all connected clients, sending the specified message to each client's
    ///     associated network stream. It logs successful message transmission or any errors encountered during the process.
    /// </remarks>
    public static void BroadcastToClients(string message)
    {
        foreach (TcpClient client in ConnectedClients)
            try
            {
                NetworkStream stream = client.GetStream();
                if (!stream.CanWrite) continue;

                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(data);

                IPEndPoint? ipEndPoint = client.GetIpv4EndPoint();
                string target = GetTargetEndPointDescription(ipEndPoint);

                LogTcpMessage($"Sent message to connected client {target}: {message}", LogType.Sent);
            }
            catch (Exception ex)
            {
                LogTcpMessage($"Error sending message to connected client: {ex.Message}", LogType.Error);
            }
    }

    /// <summary>
    ///     Logs the current connected clients.
    /// </summary>
    /// <remarks>
    ///     This method logs information about all currently connected clients, including their IPv4 endpoints.
    ///     It provides a concise overview of the connected clients for diagnostic and monitoring purposes.
    /// </remarks>
    private static void LogCurrentConnections()
    {
        try
        {
            if (ConnectedClients.Count == 0) return;

            // Extract and format the information about each connected client
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

    /// <summary>
    ///     Asynchronously monitors and removes disconnected clients at regular intervals.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the task.</param>
    /// <remarks>
    ///     Periodically checks for disconnected clients and updates the list accordingly.
    ///     Uses the provided cancellation token to gracefully stop the monitoring task.
    /// </remarks>
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
                    // If there are no connected clients, delay and continue to the next iteration.
                    await Task.Delay(timeout, cancellationToken);
                    continue;
                }

                HashSet<TcpClient?> clientsToRemove = new();

                foreach (TcpClient client in ConnectedClients)
                    try
                    {
                        if (client.IsStillConnected()) continue;

                        IPEndPoint? ipv4EndPoint = client.GetIpv4EndPoint();

                        if (ipv4EndPoint is null)
                        {
                            // If the endpoint is null, mark the client for removal.
                            clientsToRemove.Add(client);
                            continue;
                        }

                        // Check for duplicate IP addresses.
                        if (!uniqueIpAddresses.Add(ipv4EndPoint.Address))
                        {
                            LogTcpMessage($"Duplicate IP address {ipv4EndPoint} found: Removing the client...",
                                LogType.Warning);
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

                // Remove disconnected clients.
                foreach (TcpClient client in clientsToRemove.OfType<TcpClient>())
                {
                    ConnectedClients.Remove(client);
                    client.Dispose();
                }

                LogCurrentConnections();

                // Clear the set of unique IP addresses.
                if (uniqueIpAddresses.Count > 0) uniqueIpAddresses.Clear();

                // Delay before the next iteration.
                await Task.Delay(timeout, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogTcpMessage($"Error while checking connected clients: {ex.Message}", LogType.Error);
        }
    }

    /// <summary>
    ///     Listens for new messages on the provided <paramref name="stream" /> from a connected client.
    /// </summary>
    /// <param name="stream">The network stream connected to the client.</param>
    /// <param name="peer">The associated peer information.</param>
    /// <param name="cancellationToken">Cancellation token for handling task cancellation.</param>
    private static async Task ListenForNewMessages(Stream stream, Peer peer,
        CancellationToken cancellationToken)
    {
        // Set the read timeout based on the configured client timeout delay
        stream.ReadTimeout = Peer2PSettings.Instance.Timing.ClientTimeoutDelay * 2;

        byte[] buffer = new byte[4096];
        int bytesRead;

        // Continue listening for messages while data is available
        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            LogTcpMessage($"Received data from connected client {peer} - {bytesRead} bytes: Decoding...",
                LogType.Received);

            // Convert the received bytes to a string, removing newline characters
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

            // Check if the message contains a "command" property
            if (jMessage["command"] != null)
            {
                NewMessage? newMessage = jMessage.ToObject<NewMessage>();

                // Check if the deserialization was successful and the command is valid
                if (newMessage == null ||
                    newMessage.Command != Peer2PSettings.Instance.Communication.Commands.OnNewMessage)
                {
                    LogTcpMessage("Received invalid new message from connected client: Ignoring...", LogType.Warning);
                    continue;
                }

                // Process the new message and add it to the message dictionary
                NetworkData.AddMessage(newMessage, peer);

                LogTcpMessage(
                    $"Decoded message from connected client {peer}: [{newMessage.MessageId} - {newMessage.Message}]",
                    LogType.Received);

                // Send a response to the client indicating successful processing
                byte[] data = Encoding.UTF8.GetBytes(NetworkData.ReqResPair.EmptyStatus + "\n");
                await stream.WriteAsync(data, cancellationToken);

                LogTcpMessage($"Sent response on new message to {peer}: {NetworkData.ReqResPair.EmptyStatus}",
                    LogType.Sent);
                continue;
            }

            if (jMessage["status"]?.Value<string>() == Peer2PSettings.Instance.Communication.Status.OnResponse)
                LogTcpMessage($"Received status on sent new message to connected client {peer}: {message}",
                    LogType.Received);
            else
                LogTcpMessage($"Received unknown message type from connected client {peer}: {message}",
                    LogType.Expecting);
        }
    }

    /// <summary>
    ///     Stores a connected client and initiates listening for new messages.
    /// </summary>
    /// <param name="client">The TCP client representing the connected client.</param>
    /// <param name="stream">The network stream associated with the client.</param>
    /// <param name="peer">The peer information associated with the client.</param>
    /// <param name="cancellationToken">Cancellation token for handling task cancellation.</param>
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
            // Start listening for new messages on a separate task
            await ListenForNewMessages(stream, peer, cancellationToken);
            LogTcpMessage($"Timeout waiting new messages from connected client {peer}", LogType.Error);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"Error listening {peer} for new messages: {ex.Message}", LogType.Error);
        }
        finally
        {
            // Close the network stream and client after processing
            stream.Close();
            client.Close();
        }
    }
}