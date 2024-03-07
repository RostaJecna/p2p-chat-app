using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record TimingSettings
{
    private readonly int _udpDiscoveryInterval;
    private readonly int _clientTimeoutDelay;

    public int UdpDiscoveryInterval
    {
        get => _udpDiscoveryInterval;
        init
        {
            if (value <= 5000)
            {
                throw new ArgumentException($"{nameof(UdpDiscoveryInterval)} must be greater than 5000");
            }
            _udpDiscoveryInterval = value;
        }
    }

    public int ClientTimeoutDelay
    {
        get => _clientTimeoutDelay;
        init
        {
            if (value <= 5000)
            {
                throw new ArgumentException($"{nameof(ClientTimeoutDelay)} must be greater than 5000");
            }
            _clientTimeoutDelay = value;
        }
    }
}