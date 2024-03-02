using System.Diagnostics.CodeAnalysis;

namespace Peer2P.Library.Configuration.Settings;

#nullable disable
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record StatusSettings
{
    private readonly string _onResponse;

    public string OnResponse
    {
        get => _onResponse;
        init
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{nameof(OnResponse)} cannot be empty or null");
            }
            _onResponse = value;
        }
    }
}
