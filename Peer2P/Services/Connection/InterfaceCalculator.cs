using System.Net;
using System.Net.NetworkInformation;

namespace Peer2P.Services.Connection;

internal static class InterfaceCalculator
{
    public static IPAddress GetIpAddress(int interfaceId)
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        if (interfaceId > host.AddressList.Length)
            throw new ArgumentOutOfRangeException(nameof(interfaceId));

        return host.AddressList[interfaceId - 1];
    }
    
    public static IPAddress GetSubnetMask(IPAddress ipAddress)
    {
        NetworkInterface? networkInterface = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(i =>
                i.GetIPProperties().UnicastAddresses.Any(addr => Equals(addr.Address, ipAddress)));

        return networkInterface?.GetIPProperties().UnicastAddresses
            .FirstOrDefault(addr => Equals(addr.Address, ipAddress))?.IPv4Mask ?? IPAddress.None;
    }
    
    public static IPAddress GetBroadcast(IPAddress ipAddress, IPAddress subnetMask)
    {
        byte[] ipAddressBytes = ipAddress.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAddressBytes.Length != subnetMaskBytes.Length)
            throw new ArgumentException($"Lengths of IP address [{ipAddress}] and subnet mask [{subnetMask}] do not match.");

        byte[] broadcastAddress = new byte[ipAddressBytes.Length];
        for (int i = 0; i < broadcastAddress.Length; i++)
            broadcastAddress[i] = (byte)(ipAddressBytes[i] | (subnetMaskBytes[i] ^ 255));
        return new IPAddress(broadcastAddress);
    }
}