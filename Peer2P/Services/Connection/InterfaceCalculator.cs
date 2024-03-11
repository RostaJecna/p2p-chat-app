using System.Net;
using System.Net.NetworkInformation;

namespace Peer2P.Services.Connection;

/// <summary>
///     Provides methods for calculating network interface-related information.
/// </summary>
internal static class InterfaceCalculator
{
    /// <summary>
    ///     Gets the IP address associated with the specified network interface.
    /// </summary>
    /// <param name="interfaceId">The ID of the network interface.</param>
    /// <returns>The IP address associated with the network interface.</returns>
    public static IPAddress GetIpAddress(int interfaceId)
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        if (interfaceId > host.AddressList.Length)
            throw new ArgumentOutOfRangeException(nameof(interfaceId));

        return host.AddressList[interfaceId - 1];
    }

    /// <summary>
    ///     Gets the subnet mask associated with the specified IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address for which to retrieve the subnet mask.</param>
    /// <returns>The subnet mask associated with the IP address.</returns>
    public static IPAddress GetSubnetMask(IPAddress ipAddress)
    {
        NetworkInterface? networkInterface = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(i =>
                i.GetIPProperties().UnicastAddresses.Any(addr => Equals(addr.Address, ipAddress)));

        return networkInterface?.GetIPProperties().UnicastAddresses
            .FirstOrDefault(addr => Equals(addr.Address, ipAddress))?.IPv4Mask ?? IPAddress.None;
    }

    /// <summary>
    ///     Gets the broadcast address associated with the specified IP address and subnet mask.
    /// </summary>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="subnetMask">The subnet mask.</param>
    /// <returns>The broadcast address associated with the IP address and subnet mask.</returns>
    public static IPAddress GetBroadcast(IPAddress ipAddress, IPAddress subnetMask)
    {
        byte[] ipAddressBytes = ipAddress.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAddressBytes.Length != subnetMaskBytes.Length)
            throw new ArgumentException(
                $"Lengths of IP address [{ipAddress}] and subnet mask [{subnetMask}] do not match.");

        byte[] broadcastAddress = new byte[ipAddressBytes.Length];
        for (int i = 0; i < broadcastAddress.Length; i++)
            broadcastAddress[i] = (byte)(ipAddressBytes[i] | (subnetMaskBytes[i] ^ 255));
        return new IPAddress(broadcastAddress);
    }
}