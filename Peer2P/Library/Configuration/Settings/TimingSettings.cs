using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record TimingSettings
{
    private readonly int _udpDiscoveryInterval;

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
}