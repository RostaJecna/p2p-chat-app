using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

/// <summary>
///     Represents the configuration settings for timing-related intervals and delays.
/// </summary>
#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record TimingSettings
{
    private readonly int _udpDiscoveryInterval;
    private readonly int _clientTimeoutDelay;

    /// <summary>
    ///     Gets or sets the interval for UDP discovery in milliseconds.
    /// </summary>
    /// <remarks>
    ///     This interval determines how often the application performs UDP discovery to find other peers in the network.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the value is less than or equal to 5000.</exception>
    public int UdpDiscoveryInterval
    {
        get => _udpDiscoveryInterval;
        init
        {
            if (value <= 5000) throw new ArgumentException($"{nameof(UdpDiscoveryInterval)} must be greater than 5000");
            _udpDiscoveryInterval = value;
        }
    }

    /// <summary>
    ///     Gets or sets the delay for client timeout in milliseconds.
    /// </summary>
    /// <remarks>
    ///     This delay defines the time duration after which a client is considered timed out if no response or request is received.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the value is less than or equal to 5000.</exception>
    public int ClientTimeoutDelay
    {
        get => _clientTimeoutDelay;
        init
        {
            if (value <= 5000) throw new ArgumentException($"{nameof(ClientTimeoutDelay)} must be greater than 5000");
            _clientTimeoutDelay = value;
        }
    }
}