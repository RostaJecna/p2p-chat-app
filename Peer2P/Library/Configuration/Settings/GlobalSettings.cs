using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record GlobalSettings
{
    private readonly string _appPeerId;
    private readonly int _networkInterfaceId;

    public string AppPeerId
    {
        get => _appPeerId;
        init
        {
            if (string.IsNullOrEmpty(value))
            {
                value = Environment.MachineName;
            }
            
            _appPeerId = value;
        }
    }
    
    public int NetworkInterfaceId
    {
        get => _networkInterfaceId;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{nameof(NetworkInterfaceId)} must be greater than zero");
            }
            _networkInterfaceId = value;
        }
    }
}