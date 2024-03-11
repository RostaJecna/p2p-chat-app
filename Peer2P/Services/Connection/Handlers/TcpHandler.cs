using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;
using Peer2P.Library.Console.Messaging;
using Peer2P.Library.Extensions;

namespace Peer2P.Services.Connection.Handlers;

/// <summary>
///     Handles TCP connections and communication with clients.
/// </summary>
internal static class TcpHandler
{
    private static readonly TcpListener TcpListener = new(
        Peer2PSettings.Instance.Network.IpAddress,
        Peer2PSettings.Instance.Communication.BroadcastPort
    );

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
    ///     Starts listening for incoming TCP clients asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for stopping the operation.</param>
    /// <remarks>
    ///     This method initializes the TCP listener and asynchronously waits for incoming client connections.
    ///     It identifies the accepted clients, logs relevant messages, and handles them asynchronously if they are trusted
    ///     peers.
    /// </remarks>
    public static async void StartListeningAsync(CancellationToken cancellationToken)
    {
        // Start the TCP listener to accept incoming client connections
        TcpListener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient acceptedClient = await TcpListener.AcceptTcpClientAsync(cancellationToken);
                IPEndPoint? clientIpEndPoint = acceptedClient.GetIpv4EndPoint();

                LogTcpMessage($"Listener accepted unknown client [{clientIpEndPoint}]: Trying identify ...?",
                    LogType.Expecting);

                // Attempt to identify the peer associated with the accepted client
                Peer? peer = UdpHandler.TrustedPeers
                    .FirstOrDefault(kvp => acceptedClient.IsSameIpv4Address(kvp.Key.Address)).Key;

                // If the peer is not found in the trusted peers, log a warning and dispose of the client
                if (peer is null)
                {
                    LogTcpMessage($"Skip accepted client [{clientIpEndPoint}]. Is not in trusted peers.",
                        LogType.Warning);
                    acceptedClient.Dispose();
                    continue;
                }

                LogTcpMessage($"Successfully identified unknown client as: {peer}", LogType.Successful);

                // If there is an existing connection with the identified peer, log a warning and dispose of the client
                if (TcpConnections.HasConnectionWith(peer.Address))
                {
                    LogTcpMessage($"Ignored due to duplicate connection attempt: {peer}", LogType.Warning);
                    acceptedClient.Dispose();
                    continue;
                }

                HandleAcceptedClientAsync(acceptedClient, peer, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogTcpMessage($"An error occurred while listening for tcp clients: {ex.Message}", LogType.Error);
        }
    }

    /// <summary>
    ///     Handles an accepted TCP client asynchronously.
    /// </summary>
    /// <param name="client">Accepted TCP client.</param>
    /// <param name="peer">Peer associated with the accepted client.</param>
    /// <param name="cancellationToken">Cancellation token for stopping the operation.</param>
    /// <remarks>
    ///     This method is responsible for processing an accepted TCP client, waiting for a command, and responding
    ///     accordingly.
    ///     It performs a handshake with the accepted client, sending a serialized list of all messages and processing the
    ///     command.
    /// </remarks>
    private static async void HandleAcceptedClientAsync(TcpClient client, Peer peer,
        CancellationToken cancellationToken)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            LogTcpMessage($"Waiting for command from {peer}: ...?", LogType.Expecting);

            byte[] buffer = new byte[256];

            // Set up tasks to read from the stream and handle timeouts
            Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            Task timeoutTask = Task.Delay(Peer2PSettings.Instance.Timing.ClientTimeoutDelay, cancellationToken);

            // Wait for either the read operation or timeout to complete
            Task completedTask = await Task.WhenAny(readTask, timeoutTask);

            // Handle timeout case
            if (completedTask == timeoutTask)
            {
                LogTcpMessage($"Timeout waiting for command from {peer}: xxx?", LogType.Error);
                client.Dispose();
                return;
            }

            // Read the command message from the stream
            int bytesRead = await readTask;
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            CommandMessage? commandMessage = JsonConvert.DeserializeObject<CommandMessage>(message);

            // Check if the received command is valid
            if (commandMessage?.Command != Peer2PSettings.Instance.Communication.Commands.OnRequest)
            {
                LogTcpMessage($"Received invalid command message from {peer}: {message}", LogType.Received);
                client.Dispose();
                return;
            }

            LogTcpMessage($"Received command from {peer}: {commandMessage.Command}", LogType.Received);

            // Serialize all messages and send them as a response
            string allMessagesWithStatus = NetworkData.SerializeAllMessages(true, Formatting.None);
            byte[] responseBytes = Encoding.UTF8.GetBytes(allMessagesWithStatus + "\n");
            await stream.WriteAsync(responseBytes, cancellationToken);

            LogTcpMessage($"Sent messages to {peer}: [{NetworkData.AllMessages.Count}x]", LogType.Sent);
            LogTcpMessage($"Successful handshake with accepted {peer}: Send to storage...", LogType.Successful);

            // Store the connected client for further communication
            TcpConnections.StoreClientAsync(client, stream, peer, cancellationToken);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"An error occurred while handling accepted client {peer}: {ex.Message}", LogType.Error);
            client.Dispose();
        }
    }

    /// <summary>
    ///     Attempts to create a TCP client to connect with the specified peer asynchronously.
    /// </summary>
    /// <param name="peer">Peer to connect to as a TCP client.</param>
    /// <param name="cancellationToken">Cancellation token for stopping the operation.</param>
    /// <remarks>
    ///     This method initiates the asynchronous creation of a TCP client to establish a connection
    ///     with the specified peer. It handles the connection attempt, command sending, and response processing.
    ///     If the connection attempt times out or encounters an error, appropriate log entries are generated,
    ///     and the resources are disposed of. Successful connections result in the received messages being
    ///     merged into the network data, and the client is stored for further communication.
    /// </remarks>
    public static async void TryCreateTcpClientAsync(Peer peer, CancellationToken cancellationToken)
    {
        try
        {
            LogTcpMessage($"Trying to create {peer}: As TCP client...", LogType.Expecting);

            TcpClient client = new();

            // Initiate the asynchronous connection attempt and set up a timeout task
            Task connectTask = client.ConnectAsync(peer.Address.ToString(),
                Peer2PSettings.Instance.Communication.BroadcastPort);
            Task timeoutTask = Task.Delay(Peer2PSettings.Instance.Timing.ClientTimeoutDelay, cancellationToken);

            // Wait for either the connection or timeout task to complete
            Task completedConnectionTask = await Task.WhenAny(connectTask, timeoutTask);

            // Handle timeout case
            if (completedConnectionTask == timeoutTask)
            {
                LogTcpMessage($"Timeout occurred while trying to create {peer}: As TCP client...", LogType.Error);
                client.Dispose();
                return;
            }

            // Ensure the connection task is completed successfully
            await connectTask;

            LogTcpMessage($"Successfully created {peer}: (As TCP client)", LogType.Successful);

            NetworkStream stream = client.GetStream();

            // Send a command to the peer
            await stream.WriteAsync(Encoding.UTF8.GetBytes(NetworkData.ReqResPair.Command + "\n"), cancellationToken);

            LogTcpMessage($"Sent command to {peer}: {NetworkData.ReqResPair.Command}", LogType.Sent);
            LogTcpMessage($"Waiting for response from {peer}: ...?", LogType.Expecting);

            // Initiate the asynchronous read task and set up a timeout task
            byte[] buffer = new byte[Peer2PSettings.Instance.Communication.MessagesBufferSize];
            Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            // Wait for either the read task or timeout task to complete
            Task completedClientTask = await Task.WhenAny(readTask, timeoutTask);

            // Handle timeout case
            if (completedClientTask == timeoutTask)
            {
                LogTcpMessage($"Timeout waiting for response from {peer}: xxx?", LogType.Error);
                client.Dispose();
                return;
            }

            int bytesRead = await readTask;
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            TcpMessages? tcpMessages;

            try
            {
                // Try deserialize the JSON response
                tcpMessages = JsonConvert.DeserializeObject<TcpMessages>(response);
            }
            catch (JsonException ex)
            {
                LogTcpMessage($"Error deserializing JSON from {peer}: {ex.Message}" +
                              $"\n\tJSON: {response} <--", LogType.Error);
                client.Dispose();
                return;
            }

            // Check if the response is valid
            if (tcpMessages is null)
            {
                LogTcpMessage($"Received invalid response from {peer}: {response}", LogType.Error);
                client.Dispose();
                return;
            }

            // Check if the status in the response is as expected
            if (tcpMessages.Status != Peer2PSettings.Instance.Communication.Status.OnResponse)
            {
                LogTcpMessage($"Received invalid status message from {peer}: {tcpMessages.Status}", LogType.Received);
                client.Dispose();
                return;
            }

            // Check if there are messages in the response
            if (tcpMessages.Messages == null || tcpMessages.Messages.Count == 0)
            {
                LogTcpMessage($"Received empty messages from {peer}.", LogType.Received);
            }
            else
            {
                LogTcpMessage($"Received status from {peer} with messages ...?: " +
                              $"{tcpMessages.Status} - [{tcpMessages.Messages.Count}x]", LogType.Received);

                // Merge received messages into the message collection
                NetworkData.MergeMessages(tcpMessages.Messages);
            }

            LogTcpMessage($"Successful handshake with created {peer}: Send to storage...", LogType.Successful);

            // Store the connected client for further communication
            TcpConnections.StoreClientAsync(client, stream, peer, cancellationToken);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"An unexpected error occurred while trying to create {peer} (As TCP client): {ex.Message}",
                LogType.Error);
        }
    }
}