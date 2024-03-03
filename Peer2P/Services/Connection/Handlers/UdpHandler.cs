using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;
using Peer2P.Library.Console.Messaging;

namespace Peer2P.Services.Connection.Handlers;

public static class UdpHandler
{
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
        throw new NotImplementedException();
    }

    private static void HandleStatus(Peer peer)
    {
        throw new NotImplementedException();
    }
}