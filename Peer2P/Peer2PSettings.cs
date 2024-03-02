using Peer2P.Library.Configuration.Settings;

namespace Peer2P;

#nullable disable
internal sealed class Peer2PSettings
{
    private static readonly Lazy<Peer2PSettings> LazyInstance = new(() => new Peer2PSettings());
    
    public static Peer2PSettings Instance => LazyInstance.Value;
    
    public GlobalSettings Global { get; init; }
    
    public CommunicationSettings Communication { get; init; }
    
    public TimingSettings Timing { get; init; }
    
    public NetworkSettings Network { get; set; }
}