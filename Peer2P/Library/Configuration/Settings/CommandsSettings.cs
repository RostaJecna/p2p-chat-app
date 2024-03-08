using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record CommandsSettings
{
    private readonly string _onRequest;
    private readonly string _onNewMessage;
    
    public string OnRequest
    {
        get => _onRequest;
        init
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{nameof(OnRequest)} cannot be empty or null.");
            }
            _onRequest = value;
        }
    }
    
    public string OnNewMessage
    {
        get => _onNewMessage;
        init
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{nameof(OnNewMessage)} cannot be empty or null.");
            }
            _onNewMessage = value;
        }
    }
}
