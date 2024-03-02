using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Peer2P.Library.Configuration;
using Peer2P.Library.Configuration.Settings;
using Peer2P.Library.Console.Messaging;
using Peer2P.Services.Network;

namespace Peer2P.Services;

internal static class SettingsLoader
{
    public static bool TryLoadFromSection(string sectionName)
    {
        try
        {
            IConfiguration configuration = Configurator.InitBuilder().Build();
            IConfigurationSection section = configuration.GetSection(sectionName);

            if (section.Exists())
            {
                section.Bind(Peer2PSettings.Instance);
                return true;
            }

            Logger.Log(
                    $"The '{sectionName}' section does not exist in the '{Configurator.ConfigFileName}' config file.")
                .Type(LogType.Error).Display();
            return false;
        }
        catch (TargetInvocationException targetInvocationException)
        {
            string eMessage = targetInvocationException.InnerException?.Message ?? targetInvocationException.Message;
            Logger.Log(
                    $"Failed to load settings from the '{sectionName}' section in '{Configurator.ConfigFileName}' config file.")
                .Type(LogType.Error).Display();
            Logger.Log($"Error message: {eMessage}").Type(LogType.Error).Display();
            return false;
        }
        catch (Exception exception)
        {
            Logger.Log(
                    $"An unexpected error occurred while loading settings from the '{sectionName}' section in '{Configurator.ConfigFileName}' config file.")
                .Type(LogType.Error).Display();
            Logger.Log($"Error message: {exception.Message}").Type(LogType.Error).Display();
            return false;
        }
    }

    public static bool TrySetupNetInterface()
    {
        int interfaceId = Peer2PSettings.Instance.Global.NetworkInterfaceId;
        
        try
        {
            int broadcastPort = Peer2PSettings.Instance.Communication.BroadcastPort;
            
            IPAddress ipAddress = InterfaceCalculator.GetIpAddress(interfaceId);
            IPAddress subnetMask = InterfaceCalculator.GetSubnetMask(ipAddress);
            IPAddress broadcastAddress = InterfaceCalculator.GetBroadcast(ipAddress, subnetMask);
            IPEndPoint broadcastEndPoint = new(broadcastAddress, broadcastPort);
            
            Peer2PSettings.Instance.Network = new NetworkSettings(ipAddress, subnetMask, broadcastEndPoint);
            return true;
        }
        catch (Exception exception)
        {
            Logger.Log($"An unexpected error occurred while setting up the network interface. Interface ID: [{interfaceId}]")
                .Type(LogType.Error).Display();
            Logger.Log($"Error message: {exception.Message}").Type(LogType.Error).Display();
            Logger.Log($"Check if the interface specified in the '{Configurator.ConfigFileName}' config meets the conditions!")
                .Type(LogType.Warning).Display();
            return false;
        }
    }
}