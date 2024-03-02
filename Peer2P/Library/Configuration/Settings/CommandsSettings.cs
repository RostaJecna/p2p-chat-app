using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record CommandsSettings
{
    private readonly string _onRequest;
    
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
}
