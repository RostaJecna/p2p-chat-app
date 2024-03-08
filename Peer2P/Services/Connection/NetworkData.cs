using Newtonsoft.Json;
using Peer2P.Library.Connection.Json;

namespace Peer2P.Services.Connection;

internal static class NetworkData
{
    public static Dictionary<long, PeerMessage> AllMessages { get; private set; } = new();
        
    public static readonly (string Command, string Status) ReqResPair = (
        JsonConvert.SerializeObject(new CommandMessage
        {
            Command = Peer2PSettings.Instance.Communication.Commands.OnRequest,
            PeerId = Peer2PSettings.Instance.Global.AppPeerId
        }),
        JsonConvert.SerializeObject(new StatusMessage
        {
            Status = Peer2PSettings.Instance.Communication.Status.OnResponse,
            PeerId = Peer2PSettings.Instance.Global.AppPeerId
        })
    );
    
    public static string SerializeAllMessagesWithStatus()
    {
        return JsonConvert.SerializeObject(new TcpMessages
        {
            Status = Peer2PSettings.Instance.Communication.Status.OnResponse,
            Messages = AllMessages
        });
    }
    
    public static void MergeMessages(IEnumerable<KeyValuePair<long, PeerMessage>> messages)
    {
        foreach (KeyValuePair<long, PeerMessage> message in messages.Take(100))
        {
            AllMessages[message.Key] = message.Value;
        }

        AllMessages = AllMessages.Take(100).ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}