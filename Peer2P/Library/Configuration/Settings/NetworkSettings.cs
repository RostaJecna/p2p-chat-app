using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Peer2P.Library.Configuration.Settings;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record NetworkSettings(
    IPAddress IpAddress,
    IPAddress SubnetMask,
    IPEndPoint Broadcast
);