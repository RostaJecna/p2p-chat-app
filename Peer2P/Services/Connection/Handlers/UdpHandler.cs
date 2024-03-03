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
    
    public static void Handle(string? received, IPAddress sender)
    {
        try
        {
            if (string.IsNullOrEmpty(received))
            {
                Logger.Log($"Received empty or null message from [{sender}]")
                    .Type(LogType.Warning).Protocol(LogProtocol.Udp).Display();
                return;
            }
            
            HandleMessage(JObject.Parse(received), sender);
        }
        catch (JsonException ex)
        {
            Logger.Log($"Error deserializing JSON from [{sender}]: {ex.Message}")
                .Type(LogType.Error).Protocol(LogProtocol.Udp).Display();
        }
        catch (Exception ex)
        {
            Logger.Log($"Unexpected error processing message from [{sender}]: {ex.Message}")
                .Type(LogType.Error).Protocol(LogProtocol.Udp).Display();
        }
    }

    private static void HandleMessage(JObject message, IPAddress sender)
    {
        string? peerId = message["peer_id"]?.Value<string>();

        if (string.IsNullOrWhiteSpace(peerId))
        {
            Logger.Log($"Received message from [{sender}] with invalid peer id: {message.ToString(Formatting.None)}")
                .Type(LogType.Warning).Protocol(LogProtocol.Udp).Display();
            return;
        }

        Peer peer = new(peerId, sender);
        
        if (message["command"] != null)
        {
            CommandMessage? command = message.ToObject<CommandMessage>();
            if (command != null && command.Command == Peer2PSettings.Instance.Communication.Commands.OnRequest)
            {
                Logger.Log($"Received command message from {peer}: {command.Command}")
                    .Type(LogType.Received).Protocol(LogProtocol.Udp).Display();
                HandleCommand(peer);
                return;
            }

            Logger.Log($"Received invalid command message from {peer}: {message.ToString(Formatting.None)}")
                .Type(LogType.Received).Protocol(LogProtocol.Udp).Display();
        }
        else if (message["status"] != null)
        {
            StatusMessage? status = message.ToObject<StatusMessage>();
            if (status != null && status.Status == Peer2PSettings.Instance.Communication.Status.OnResponse)
            {
                Logger.Log($"Received status message from {peer}: {status.Status}")
                    .Type(LogType.Received).Protocol(LogProtocol.Udp).Display();
                HandleStatus(peer);
                return;
            }

            Logger.Log($"Received invalid status message from {peer}: {message.ToString(Formatting.None)}")
                .Type(LogType.Received).Protocol(LogProtocol.Udp).Display();
        }
        else
        {
            Logger.Log($"Received unknown message type from {peer}: {message.ToString(Formatting.None)}")
                .Type(LogType.Received).Protocol(LogProtocol.Udp).Display();
        }
    }
    
    private static void HandleCommand(Peer peer)
    {
        TrustedPeers.Add(peer);
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
            
            foreach (Peer peer in TrustedPeers.Keys.Where(peer => TrustedPeers.GetTimeDifference(peer) > interval))
            {
                TrustedPeers.Remove(peer);
                Logger.Log($"Removed peer {peer} from trusted list due to inactivity!")
                    .Type(LogType.Warning).Protocol(LogProtocol.Udp).Display();
            }
            
            string peers = string.Join(", ", TrustedPeers);
            Logger.Log($"Trusted peers ({TrustedPeers.Count}) - {peers}")
                .Type(LogType.Expecting).Display();

            await Task.Delay(interval, cancellationToken);
        }
    }
}