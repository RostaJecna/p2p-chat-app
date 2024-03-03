using Newtonsoft.Json;
using Peer2P.Library.Connection.Json;

namespace Peer2P.Library.Connection;

internal static class NetMessages
{
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
}