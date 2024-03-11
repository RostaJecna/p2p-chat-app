using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Peer2P.Library.Configuration.Settings;

/// <summary>
///     Represents the network configuration settings for the peer-to-peer network application.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record NetworkSettings(
    IPAddress IpAddress,
    IPAddress SubnetMask,
    IPEndPoint Broadcast
);