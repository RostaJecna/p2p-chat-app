using Peer2P.Library.Configuration.Settings;

namespace Peer2P;

#nullable disable
internal sealed class Peer2PSettings
{
    private static readonly Lazy<Peer2PSettings> LazyInstance = new(() => new Peer2PSettings());

    /// <summary>
    ///     Gets the singleton instance of the Peer2PSettings.
    /// </summary>
    public static Peer2PSettings Instance => LazyInstance.Value;

    /// <summary>
    ///     Gets or initializes the global settings.
    /// </summary>
    public GlobalSettings Global { get; init; }

    /// <summary>
    ///     Gets or initializes the communication settings.
    /// </summary>
    public CommunicationSettings Communication { get; init; }

    /// <summary>
    ///     Gets or initializes the timing settings.
    /// </summary>
    public TimingSettings Timing { get; init; }

    /// <summary>
    ///     Gets or sets the network settings.
    /// </summary>
    public NetworkSettings Network { get; set; }

    /// <summary>
    ///     Private constructor to prevent external instantiation.
    /// </summary>
    private Peer2PSettings()
    {
    }
}