using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Peer2P.Library.Collections;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;
using Peer2P.Library.Console.Messaging;

namespace Peer2P.Services.Connection.Handlers;

/// <summary>
///     Handles UDP-related operations, such as parsing messages, managing trusted peers, and handling commands and
///     statuses.
/// </summary>
internal static class UdpHandler
{
    /// <summary>
    ///     Collection of trusted peers along with their last activity timestamp.
    /// </summary>
    public static readonly TimeStorage<Peer> TrustedPeers = new();

    /// <summary>
    ///     Logs a UDP-related message with the specified message and log type.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="type">The log type.</param>
    private static void LogUdpMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Udp).Display();
    }

    /// <summary>
    ///     Handles an incoming UDP message, parsing the received JSON string and processing the message accordingly.
    /// </summary>
    /// <param name="received">The received JSON message as a string.</param>
    /// <param name="sender">The IP address of the message sender.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <remarks>
    ///     This method checks for null or empty incoming messages and logs warnings for such cases. It then attempts to
    ///     parse the JSON string into a JObject. If successful, the method delegates further processing to the
    ///     <see cref="HandleMessage" /> method. If parsing fails due to a JSON exception, an error message is logged.
    ///     Any unexpected exceptions during the processing of the message are also logged as errors.
    /// </remarks>
    public static void Handle(string? received, IPAddress sender, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(received))
            {
                LogUdpMessage($"Received empty or null message from [{sender}]", LogType.Warning);
                return;
            }

            JObject jReceived = JObject.Parse(received);
            HandleMessage(jReceived, sender, cancellationToken);
        }
        catch (JsonException ex)
        {
            LogUdpMessage($"Error parsing to {nameof(JObject)} from [{sender}]: {ex.Message}", LogType.Error);
        }
        catch (Exception ex)
        {
            LogUdpMessage($"Got unexpected error processing message from [{sender}]: {ex.Message}", LogType.Error);
        }
    }

    /// <summary>
    ///     Handles a parsed JSON message received over UDP, extracting relevant information and delegating further processing.
    /// </summary>
    /// <param name="jMessage">The parsed JObject representing the received message.</param>
    /// <param name="sender">The IP address of the message sender.</param>
    /// <param name="cancellationToken">Cancellation token for handling task cancellation.</param>
    /// <remarks>
    ///     This method extracts the peer ID and checks for its validity. It then examines the message type (command, status,
    ///     or unknown) and dispatches the processing to specialized methods such as <see cref="HandleCommand" /> and
    ///     <see cref="HandleStatus" />.
    /// </remarks>
    private static void HandleMessage(JObject jMessage, IPAddress sender, CancellationToken cancellationToken)
    {
        string? peerId = jMessage["peer_id"]?.Value<string>();
        string jMessageFormatted = jMessage.ToString(Formatting.None);

        if (string.IsNullOrWhiteSpace(peerId))
        {
            LogUdpMessage($"Received message from [{sender}] with invalid peer id: {jMessageFormatted}",
                LogType.Warning);
            return;
        }

        Peer peer = new(peerId, sender);

        if (jMessage["command"] != null)
        {
            CommandMessage? command = jMessage.ToObject<CommandMessage>();
            if (command != null && command.Command == Peer2PSettings.Instance.Communication.Commands.OnRequest)
            {
                LogUdpMessage($"Received command message from {peer}: {command.Command}", LogType.Received);
                HandleCommand(peer);
                return;
            }

            LogUdpMessage($"Received invalid command message from {peer}: {jMessageFormatted}", LogType.Received);
        }
        else if (jMessage["status"] != null)
        {
            StatusMessage? status = jMessage.ToObject<StatusMessage>();
            if (status != null && status.Status == Peer2PSettings.Instance.Communication.Status.OnResponse)
            {
                LogUdpMessage($"Received status message from {peer}: {status.Status}", LogType.Received);
                HandleStatus(peer, cancellationToken);
                return;
            }

            LogUdpMessage($"Received invalid status message from {peer}: {jMessageFormatted}", LogType.Received);
        }
        else
        {
            LogUdpMessage($"Received unknown message type from {peer}: {jMessageFormatted}", LogType.Received);
        }
    }

    /// <summary>
    ///     Handles a command message received over UDP, storing the sender as a trusted peer and responding with a status
    ///     message.
    /// </summary>
    /// <param name="peer">The peer information extracted from the received message.</param>
    /// <remarks>
    ///     This method stores the sender as a trusted peer, checks for existing TCP connections, and sends a status response
    ///     via UDP if no TCP connection exists.
    /// </remarks>
    private static void HandleCommand(Peer peer)
    {
        TrustedPeers.Store(peer);

        if (!TcpConnections.HasConnectionWith(peer.Address))
        {
            UdpDiscovery.SendTo(NetworkData.ReqResPair.Status + "\n",
                new IPEndPoint(peer.Address, Peer2PSettings.Instance.Communication.BroadcastPort),
                Encoding.ASCII);
            LogUdpMessage($"Sent status response to {peer}: {NetworkData.ReqResPair.Status}", LogType.Sent);
        }
        else
        {
            LogUdpMessage($"Tcp connection with {peer} already exists.", LogType.Warning);
        }
    }

    /// <summary>
    ///     Handles a status message received over UDP, storing the sender as a trusted peer and attempting to establish a TCP
    ///     connection.
    /// </summary>
    /// <param name="peer">The peer information extracted from the received message.</param>
    /// <param name="cancellationToken">Cancellation token for handling task cancellation.</param>
    /// <remarks>
    ///     This method stores the sender as a trusted peer and attempts to establish a TCP connection if none exists.
    /// </remarks>
    private static void HandleStatus(Peer peer, CancellationToken cancellationToken)
    {
        TrustedPeers.Store(peer);

        if (!TcpConnections.HasConnectionWith(peer.Address))
            TcpHandler.TryCreateTcpClientAsync(peer, cancellationToken);
        else
            LogUdpMessage($"Tcp connection with {peer} already exists.", LogType.Warning);
    }

    /// <summary>
    ///     Periodically checks the activity of trusted peers and removes inactive ones from the list.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for handling task cancellation.</param>
    /// <remarks>
    ///     This method runs in a loop, checking the activity of trusted peers at intervals. If a peer has been inactive for
    ///     a duration exceeding the specified interval, it is removed from the trusted peers list. The method logs the
    ///     activity status and the list of trusted peers for monitoring purposes.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for handling task cancellation.</param>
    public static async void HandlePeriodicTrustedPeersAsync(CancellationToken cancellationToken)
    {
        int interval = Peer2PSettings.Instance.Timing.UdpDiscoveryInterval * 3;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (TrustedPeers.Count == 0)
            {
                await Task.Delay(interval, cancellationToken);
                continue;
            }

            foreach (Peer peer in TrustedPeers.Keys.Where(peer =>
                         TrustedPeers.GetTimeDifferenceMilliseconds(peer) > interval))
            {
                TrustedPeers.TryRemove(peer, out _);
                LogUdpMessage($"Removed peer {peer} from trusted list due to inactivity!", LogType.Warning);
            }

            string peers = string.Join(", ", TrustedPeers);
            LogUdpMessage($"Trusted peers ({TrustedPeers.Count}) - {peers}", LogType.Expecting);

            await Task.Delay(interval, cancellationToken);
        }
    }

    /// <summary>
    ///     Gets the peer ID associated with a given IP address from the list of trusted peers.
    /// </summary>
    /// <param name="address">The IP address to search for in the trusted peers list.</param>
    /// <returns>The peer ID if found; otherwise, <c>null</c>.</returns>
    public static string? GetPeerIdByAddress(IPAddress? address)
    {
        return TrustedPeers.Keys.FirstOrDefault(peer => peer.Address.Equals(address))?.Id;
    }
}