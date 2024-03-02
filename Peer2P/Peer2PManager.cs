using Peer2P.Library.Console.Messaging;
using Peer2P.Services;

namespace Peer2P;

public static class Peer2PManager
{
    public static bool TryInitialize()
    {
        if (!SettingsLoader.TryLoadFromSection("Peer2P") || !SettingsLoader.TrySetupNetInterface())
        {
            Logger.Log("Failed to load settings for the Peer2P library.")
                .Type(LogType.Error).Display();
            return false;
        }
        
        Logger.Log("The settings were successfully loaded for the Peer2P library.")
            .Type(LogType.Successful).Display();
        
        
        
        return true;
    }
}