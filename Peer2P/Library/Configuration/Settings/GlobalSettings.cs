using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

/// <summary>
///     Represents the global configuration settings for the peer-to-peer network application.
/// </summary>
#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record GlobalSettings
{
    private readonly string _appPeerId;
    private readonly int _networkInterfaceId;

    /// <summary>
    ///     Gets or sets the unique identifier for the application's peer.
    /// </summary>
    /// <remarks>
    ///     If not specified, the default value is the machine name.
    /// </remarks>
    public string AppPeerId
    {
        get => _appPeerId;
        init
        {
            if (string.IsNullOrEmpty(value)) value = Environment.MachineName;

            _appPeerId = value;
        }
    }

    /// <summary>
    ///     Gets or sets the identifier for the network interface used by the application.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the value is less than or equal to zero.</exception>
    public int NetworkInterfaceId
    {
        get => _networkInterfaceId;
        init
        {
            if (value <= 0) throw new ArgumentException($"{nameof(NetworkInterfaceId)} must be greater than zero");
            _networkInterfaceId = value;
        }
    }
}