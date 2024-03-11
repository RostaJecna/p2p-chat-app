using Newtonsoft.Json;
using Peer2P.Library.Connection;
using Peer2P.Library.Connection.Json;

namespace Peer2P.Services.Connection;

/// <summary>
///     Manages network-related data, including messages and serialization operations.
/// </summary>
public static class NetworkData
{
    /// <summary>
    ///     Gets or sets a dictionary containing all messages with their unique IDs.
    /// </summary>
    public static Dictionary<long, PeerMessage> AllMessages { get; private set; } = new();

    /// <summary>
    ///     Gets a tuple containing serialized representations of command, status, and empty status messages.
    /// </summary>
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
            status = Peer2PSettings.Instance.Communication.Status.OnResponse
        })
    );

    /// <summary>
    ///     Serializes all messages based on the specified status and formatting options.
    /// </summary>
    /// <param name="status">If true, includes status information in the serialization.</param>
    /// <param name="formatting">The formatting options for the JSON serialization.</param>
    /// <returns>The serialized representation of all messages.</returns>
    public static string SerializeAllMessages(bool status, Formatting formatting)
    {
        if (status)
            return JsonConvert.SerializeObject(new TcpMessages
            {
                Status = Peer2PSettings.Instance.Communication.Status.OnResponse,
                Messages = AllMessages
            }, formatting);

        return JsonConvert.SerializeObject(new
        {
            messages = AllMessages
        }, formatting);
    }


    /// <summary>
    ///     Merges the provided messages into the collection of all messages.
    /// </summary>
    /// <param name="messages">The messages to merge into the collection.</param>
    public static void MergeMessages(IEnumerable<KeyValuePair<long, PeerMessage>> messages)
    {
        foreach (KeyValuePair<long, PeerMessage> message in messages) AllMessages[message.Key] = message.Value;

        AllMessages = AllMessages
            .OrderByDescending(pair => pair.Key)
            .Take(Peer2PSettings.Instance.Communication.MaxMessages)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    /// <summary>
    ///     Adds a new message along with its associated peer to the collection of all messages.
    /// </summary>
    /// <param name="message">The new message and its associated peer.</param>
    /// <param name="peer">The peer associated with the new message.</param>
    public static void AddMessage(NewMessage message, Peer peer)
    {
        AllMessages[message.MessageId] = new PeerMessage
        {
            PeerId = peer.Id,
            Message = message.Message
        };
    }

    /// <summary>
    ///     Adds a new message with the specified ID and content to the collection of all messages.
    /// </summary>
    /// <param name="messageId">The ID of the new message.</param>
    /// <param name="message">The content of the new message.</param>
    public static void AddMessage(long messageId, string message)
    {
        AllMessages[messageId] = new PeerMessage
        {
            PeerId = Peer2PSettings.Instance.Global.AppPeerId,
            Message = message
        };
    }

    /// <summary>
    ///     Serializes a new message with the specified ID and content.
    /// </summary>
    /// <param name="messageId">The ID of the new message.</param>
    /// <param name="message">The content of the new message.</param>
    /// <returns>The serialized representation of the new message.</returns>
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