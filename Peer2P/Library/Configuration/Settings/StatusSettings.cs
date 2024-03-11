using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

/// <summary>
///     Represents the configuration settings related to the status of communication in the peer-to-peer network.
/// </summary>
#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record StatusSettings
{
    private readonly string _onResponse;

    /// <summary>
    ///     Gets or sets the status used for response communication.
    /// </summary>
    public string OnResponse
    {
        get => _onResponse;
        init
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{nameof(OnResponse)} cannot be empty or null");
            _onResponse = value;
        }
    }
}