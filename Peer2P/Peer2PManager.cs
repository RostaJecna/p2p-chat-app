using Peer2P.Library.Connection;
using Peer2P.Library.Console.Messaging;
using Peer2P.Services;
using Peer2P.Services.Connection;

namespace Peer2P;

public static class Peer2PManager
{
    public static bool TryInitialize(CancellationToken cancellationToken)
    {
        if (!SettingsLoader.TryLoadFromSection("Peer2P") || !SettingsLoader.TrySetupNetInterface())
        {
            Logger.Log("Failed to load settings for the Peer2P library.")
                .Type(LogType.Error).Display();
            return false;
        }
        
        try
        {
            UdpDiscovery.SendPeriodicAsync(NetMessages.ReqResPair.Command, cancellationToken);
            UdpDiscovery.ListenIncomingAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log($"An unexpected error occurred while running services in the Peer2P library: {ex.Message}")
                .Type(LogType.Error).Display();
            UdpDiscovery.Dispose();
            return false;
        }
    }
}