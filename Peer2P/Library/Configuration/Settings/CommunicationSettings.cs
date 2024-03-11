using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

/// <summary>
///     Represents the configuration settings for communication in the peer-to-peer network.
/// </summary>
#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record CommunicationSettings
{
    private readonly int _broadcastPort;
    private readonly ushort _messagesBufferSize;
    private readonly byte _maxMessages;
    private readonly CommandsSettings _commands;
    private readonly StatusSettings _status;


    /// <summary>
    ///     Gets or sets the port used for broadcasting messages.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the value is not in the range of 1 to 65535.</exception>
    public int BroadcastPort
    {
        get => _broadcastPort;
        init
        {
            if (value is < 1 or > 65535)
                throw new ArgumentException($"{nameof(BroadcastPort)} must be in the range of 1 to 65535");
            _broadcastPort = value;
        }
    }

    /// <summary>
    ///     Gets or sets the buffer size for incoming messages.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the value is less than 4096.</exception>
    public ushort MessagesBufferSize
    {
        get => _messagesBufferSize;
        init
        {
            if (value < 4096) throw new ArgumentException($"{nameof(MessagesBufferSize)} must be at least 4096");
            _messagesBufferSize = value;
        }
    }

    /// <summary>
    ///     Gets or sets the maximum number of messages that can be stored in the collection.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the value is less than 10.</exception>
    public byte MaxMessages
    {
        get => _maxMessages;
        init
        {
            if (value < 10) throw new ArgumentException($"{nameof(MaxMessages)} must be at least 10");
            _maxMessages = value;
        }
    }

    /// <summary>
    ///     Gets or sets the settings for handling commands.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
    public CommandsSettings Commands
    {
        get => _commands;
        init => _commands = value ?? throw new ArgumentNullException(nameof(Commands));
    }

    /// <summary>
    ///     Gets or sets the settings related to the status of the communication.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
    public StatusSettings Status
    {
        get => _status;
        init => _status = value ?? throw new ArgumentNullException(nameof(Status));
    }
}