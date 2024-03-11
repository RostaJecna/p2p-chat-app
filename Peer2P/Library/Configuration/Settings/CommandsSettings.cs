using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

/// <summary>
///     Represents the configuration settings for handling commands.
/// </summary>
#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record CommandsSettings
{
    private readonly string _onRequest;
    private readonly string _onNewMessage;

    /// <summary>
    ///     Gets or sets the command used for request communication.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the value is empty or null.</exception>
    public string OnRequest
    {
        get => _onRequest;
        init
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{nameof(OnRequest)} cannot be empty or null.");
            _onRequest = value;
        }
    }

    /// <summary>
    ///     Gets or sets the command used for new message communication
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the value is empty or null.</exception>
    public string OnNewMessage
    {
        get => _onNewMessage;
        init
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{nameof(OnNewMessage)} cannot be empty or null.");
            _onNewMessage = value;
        }
    }
}