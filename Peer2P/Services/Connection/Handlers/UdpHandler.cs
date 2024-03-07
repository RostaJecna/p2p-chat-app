using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Peer2P.Library.Collections;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;
using Peer2P.Library.Console.Messaging;

namespace Peer2P.Services.Connection.Handlers;

internal static class UdpHandler
{
    public static readonly TimeStorage<Peer> TrustedPeers = new();

    private static void LogUdpMessage(string message, LogType type)
    {
        Logger.Log(message).Type(type).Protocol(LogProtocol.Udp).Display();
    }

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
                HandleCommand(peer, cancellationToken);
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
                HandleStatus(peer);
                return;
            }

            LogUdpMessage($"Received invalid status message from {peer}: {jMessageFormatted}", LogType.Received);
        }
        else
        {
            LogUdpMessage($"Received unknown message type from {peer}: {jMessageFormatted}", LogType.Received);
        }
    }

    private static void HandleCommand(Peer peer, CancellationToken cancellationToken)
    {
        TrustedPeers.Add(peer);

        if (!TcpHandler.HasConnectionWith(peer.Address))
            TcpHandler.TryCreateTcpClientAsync(peer, cancellationToken);
        else
            LogUdpMessage($"Tcp connection with {peer} already exists.", LogType.Warning);
    }

    private static void HandleStatus(Peer peer)
    {
        TrustedPeers.Add(peer);
    }

    public static async void HandlePeriodicTrustedPeersAsync(CancellationToken cancellationToken)
    {
        int interval = Peer2PSettings.Instance.Timing.UdpDiscoveryInterval * 3;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (TrustedPeers.Count == 0) await Task.Delay(interval, cancellationToken);

            foreach (Peer peer in TrustedPeers.Keys.Where(peer =>
                         TrustedPeers.GetTimeDifferenceMilliseconds(peer) > interval))
            {
                TrustedPeers.Remove(peer);
                LogUdpMessage($"Removed peer {peer} from trusted list due to inactivity!", LogType.Warning);
            }

            string peers = string.Join(", ", TrustedPeers);
            LogUdpMessage($"Trusted peers ({TrustedPeers.Count}) - {peers}", LogType.Expecting);

            await Task.Delay(interval, cancellationToken);
        }
    }
}