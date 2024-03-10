using Newtonsoft.Json;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;

namespace Peer2P.Services.Connection;

public static class NetworkData
{
    public static Dictionary<long, PeerMessage> AllMessages { get; private set; } = new();
        
    public static readonly (string Command, string Status, string EmptyStatus) ReqResPair = (
        JsonConvert.SerializeObject(new CommandMessage
        {
            Command = Peer2PSettings.Instance.Communication.Commands.OnRequest,
            PeerId = Peer2PSettings.Instance.Global.AppPeerId
        }),
        JsonConvert.SerializeObject(new StatusMessage
        {
            Status = Peer2PSettings.Instance.Communication.Status.OnResponse,
            PeerId = Peer2PSettings.Instance.Global.AppPeerId
        }),
        JsonConvert.SerializeObject(new
        {
            status = Peer2PSettings.Instance.Communication.Status.OnResponse,
        })
    );
    
    public static string SerializeAllMessages(bool status, Formatting formatting)
    {
        if (status)
        {
            return JsonConvert.SerializeObject(new TcpMessages
            {
                Status = Peer2PSettings.Instance.Communication.Status.OnResponse,
                Messages = AllMessages
            }, formatting);
        }
        
        return JsonConvert.SerializeObject(new
        {
            messages = AllMessages
        }, formatting);
    }
    
    public static void MergeMessages(IEnumerable<KeyValuePair<long, PeerMessage>> messages)
    {
        foreach (KeyValuePair<long, PeerMessage> message in messages)
        {
            AllMessages[message.Key] = message.Value;
        }

        AllMessages = AllMessages
            .OrderByDescending(pair => pair.Key)
            .Take(Peer2PSettings.Instance.Communication.MaxMessages)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
    
    public static void AddMessage(NewMessage message, Peer peer)
    {
        AllMessages[message.MessageId] = new PeerMessage
        {
            PeerId = peer.Id,
            Message = message.Message
        };
    }
    
    public static void AddMessage(long messageId, string message)
    {
        AllMessages[messageId] = new PeerMessage
        {
            PeerId = Peer2PSettings.Instance.Global.AppPeerId,
            Message = message
        };
    }
    
    public static string SerializeNewMessage(long messageId, string message)
    {
        return JsonConvert.SerializeObject(new NewMessage
        {
            Command = Peer2PSettings.Instance.Communication.Commands.OnNewMessage,
            MessageId = messageId,
            Message = message
        });
    }
}