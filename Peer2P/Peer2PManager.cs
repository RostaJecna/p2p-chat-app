using Peer2P.Library.Console.Messaging;
using Peer2P.Services;
using Peer2P.Services.Connection;
using Peer2P.Services.Connection.Handlers;

namespace Peer2P;

/// <summary>
/// Main class for initializing the Peer2P library.
/// </summary>
public static class Peer2PManager
{
    /// <summary>
    /// Tries to initialize the Peer2P library with the provided cancellation token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for stopping the initialization process.</param>
    /// <returns>Returns true if initialization is successful; otherwise, false.</returns>
    public static bool TryInitialize(CancellationToken cancellationToken)
    {
        // Try loading settings and setting up the network interface
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
            // Start necessary services and handlers
            TcpHandler.StartListeningAsync(cancellationToken);
            TcpConnections.StartCheckConnectedClientsAsync(cancellationToken);
            UdpHandler.HandlePeriodicTrustedPeersAsync(cancellationToken);

            UdpDiscovery.SendPeriodicAsync(NetworkData.ReqResPair.Command, cancellationToken);
            UdpDiscovery.ListenIncomingAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log($"An unexpected error occurred while starting services in the Peer2P library: {ex.Message}")
                .Type(LogType.Error).Display();
            return false;
        }
    }
}