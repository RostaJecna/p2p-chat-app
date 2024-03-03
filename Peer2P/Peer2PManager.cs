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
        
        Logger.Log("Starting Peer2P services with the following settings:")
            .Type(LogType.Expecting).Display();
        Logger.Log($"\tIP: {Peer2PSettings.Instance.Network.IpAddress}," +
                   $"\n\tSubnet: {Peer2PSettings.Instance.Network.SubnetMask}," +
                   $"\n\tBroadcast: {Peer2PSettings.Instance.Network.Broadcast}")
            .Display();
        
        try
        {
            UdpDiscovery.SendPeriodicAsync(NetMessages.ReqResPair.Command, cancellationToken);
            UdpDiscovery.ListenIncomingAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log($"An unexpected error occurred while starting services in the Peer2P library: {ex.Message}")
                .Type(LogType.Error).Display();
            UdpDiscovery.Dispose();
            return false;
        }
    }
}