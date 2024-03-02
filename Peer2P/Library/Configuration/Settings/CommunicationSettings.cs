using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record CommunicationSettings
{
    private readonly int _broadcastPort;
    private readonly CommandsSettings _commands;
    private readonly StatusSettings _status;

    public int BroadcastPort
    {
        get => _broadcastPort;
        init
        {
            if (value is < 1 or > 65535)
            {
                throw new ArgumentException($"{nameof(BroadcastPort)} must be in the range of 1 to 65535");
            }
            _broadcastPort = value;
        }
    }

    public CommandsSettings Commands
    {
        get => _commands;
        init => _commands = value ?? throw new ArgumentNullException(nameof(Commands));
    }

    public StatusSettings Status
    {
        get => _status;
        init => _status = value ?? throw new ArgumentNullException(nameof(Status));
    }
}