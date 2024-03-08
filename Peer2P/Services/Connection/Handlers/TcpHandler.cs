using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;
using Peer2P.Library.Console.Messaging;
using Peer2P.Library.Extensions;

namespace Peer2P.Services.Connection.Handlers;

internal static class TcpHandler
{
    private static readonly TcpListener TcpListener = new(
        Peer2PSettings.Instance.Network.IpAddress,
        Peer2PSettings.Instance.Communication.BroadcastPort
    );
    
    private static void LogTcpMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Tcp).Display();
    }

    public static async void StartListeningAsync(CancellationToken cancellationToken)
    {
        TcpListener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient acceptedClient = await TcpListener.AcceptTcpClientAsync(cancellationToken);
                IPEndPoint? clientIpEndPoint = acceptedClient.GetIpv4EndPoint();

                LogTcpMessage($"Listener accepted unknown client [{clientIpEndPoint}]: Trying identify ...?",
                    LogType.Expecting);

                Peer? peer = UdpHandler.TrustedPeers
                    .FirstOrDefault(kvp => acceptedClient.IsSameIpv4Address(kvp.Key.Address)).Key;
                
                if (peer is null)
                {
                    LogTcpMessage($"Skip accepted client [{clientIpEndPoint}]. Is not in trusted peers.",
                        LogType.Warning);
                    acceptedClient.Dispose();
                    continue;
                }

                LogTcpMessage($"Successfully identified unknown client as: {peer}", LogType.Successful);
                
                if (TcpConnections.HasConnectionWith(clientIpEndPoint?.Address))
                {
                    LogTcpMessage($"Ignored due to duplicate connection attempt: {peer}", LogType.Warning);
                    return;
                }

                HandleAcceptedClientAsync(acceptedClient, peer, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogTcpMessage($"An error occurred while listening for tcp clients: {ex.Message}", LogType.Error);
        }
    }

    private static async void HandleAcceptedClientAsync(TcpClient client, Peer peer,
        CancellationToken cancellationToken)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            LogTcpMessage($"Waiting for command from {peer}: ...?", LogType.Expecting);

            byte[] buffer = new byte[1024];

            Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            Task timeoutTask = Task.Delay(Peer2PSettings.Instance.Timing.ClientTimeoutDelay, cancellationToken);

            Task completedTask = await Task.WhenAny(readTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                LogTcpMessage($"Timeout waiting for command from {peer}: xxx?", LogType.Error);
                client.Dispose();
                return;
            }

            int bytesRead = await readTask;
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            CommandMessage? commandMessage = JsonConvert.DeserializeObject<CommandMessage>(message);

            if (commandMessage?.Command != Peer2PSettings.Instance.Communication.Commands.OnRequest)
            {
                LogTcpMessage($"Received invalid command message from {peer}: {message}", LogType.Received);
                client.Dispose();
                return;
            }

            LogTcpMessage($"Received command from {peer}: {commandMessage.Command}", LogType.Received);

            string allMessagesWithStatus = NetworkData.SerializeAllMessagesWithStatus();

            byte[] responseBytes = Encoding.UTF8.GetBytes(allMessagesWithStatus + "\n");
            await stream.WriteAsync(responseBytes, cancellationToken);

            LogTcpMessage($"Sent messages to {peer}: [{NetworkData.AllMessages.Count}x]", LogType.Sent);
            LogTcpMessage($"Successful handshake with accepted {peer}: Send to storage...", LogType.Successful);

            TcpConnections.StoreClient(client, stream);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"An error occurred while handling accepted client {peer}: {ex.Message}", LogType.Error);
            client.Dispose();
        }
    }

    public static async void TryCreateTcpClientAsync(Peer peer, CancellationToken cancellationToken)
    {
        try
        {
            LogTcpMessage($"Trying to create {peer}: As TCP client...", LogType.Expecting);

            TcpClient client = new();

            // ReSharper disable once MethodSupportsCancellation
            Task connectTask = client.ConnectAsync(peer.Address.ToString(),
                Peer2PSettings.Instance.Communication.BroadcastPort);
            Task timeoutTask = Task.Delay(Peer2PSettings.Instance.Timing.ClientTimeoutDelay, cancellationToken);

            Task completedConnectionTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedConnectionTask == timeoutTask)
            {
                LogTcpMessage($"Timeout occurred while trying to create {peer}: As TCP client...", LogType.Error);
                client.Dispose();
                return;
            }

            await connectTask;

            LogTcpMessage($"Successfully created {peer}: (As TCP client)", LogType.Successful);

            NetworkStream stream = client.GetStream();

            await stream.WriteAsync(Encoding.UTF8.GetBytes(NetworkData.ReqResPair.Command + "\n"), cancellationToken);

            LogTcpMessage($"Sent command to {peer}: {NetworkData.ReqResPair.Command}", LogType.Sent);
            LogTcpMessage($"Waiting for response from {peer}: ...?", LogType.Expecting);

            // TODO: Use memory read buffer
            byte[] buffer = new byte[12288];
            Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            Task completedClientTask = await Task.WhenAny(readTask, timeoutTask);

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
                tcpMessages = JsonConvert.DeserializeObject<TcpMessages>(response);
            }
            catch (JsonException ex)
            {
                LogTcpMessage($"Error deserializing JSON from {peer}: {ex.Message}" +
                              $"\n\tJSON: {response} <--", LogType.Error);
                client.Dispose();
                return;
            }

            if (tcpMessages is null)
            {
                LogTcpMessage($"Received invalid response from {peer}: {response}", LogType.Error);
                client.Dispose();
                return;
            }

            if (tcpMessages.Status != Peer2PSettings.Instance.Communication.Status.OnResponse)
            {
                LogTcpMessage($"Received invalid status message from {peer}: {tcpMessages.Status}", LogType.Received);
                client.Dispose();
                return;
            }

            if (tcpMessages.Messages == null || tcpMessages.Messages.Count == 0)
            {
                LogTcpMessage($"Received empty messages from {peer}.", LogType.Received);
            }
            else
            {
                LogTcpMessage($"Received status from {peer} with messages ...?: " +
                              $"{tcpMessages.Status} - [{tcpMessages.Messages.Count}x]", LogType.Received);

                NetworkData.MergeMessages(tcpMessages.Messages);
            }
            
            LogTcpMessage($"Successful handshake with created {peer}: Send to storage...", LogType.Successful);

            TcpConnections.StoreClient(client, stream);
        }
        catch (Exception ex)
        {
            LogTcpMessage($"An unexpected error occurred while trying to create {peer} (As TCP client): {ex.Message}",
                LogType.Error);
        }
    }
}