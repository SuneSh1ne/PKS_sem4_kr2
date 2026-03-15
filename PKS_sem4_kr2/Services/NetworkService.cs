using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using PKS_sem4_kr2.Models;

namespace PKS_sem4_kr2.Services
{
    public class NetworkService
    {
        public List<NetworkInterfaceInfo> GetNetworkInterfaces()
        {
            var interfaces = new List<NetworkInterfaceInfo>();
            
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up && 
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    continue;

                var ipProperties = ni.GetIPProperties();
                var ipAddressInfo = ipProperties.UnicastAddresses
                    .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddressInfo == null) continue;

                var interfaceInfo = new NetworkInterfaceInfo
                {
                    Name = ni.Name,
                    Description = ni.Description,
                    IpAddress = ipAddressInfo.Address.ToString(),
                    SubnetMask = ipAddressInfo.IPv4Mask?.ToString() ?? "Н/Д",
                    MacAddress = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()),
                    Status = ni.OperationalStatus,
                    Speed = ni.Speed,
                    InterfaceType = ni.NetworkInterfaceType
                };

                interfaces.Add(interfaceInfo);
            }

            return interfaces;
        }

        public async Task<(bool Success, long RoundtripTime)> PingHostAsync(string host)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(host, 3000);
                    return (reply.Status == IPStatus.Success, reply.RoundtripTime);
                }
            }
            catch
            {
                return (false, 0);
            }
        }

        public async Task<DnsInfo> GetDnsInfoAsync(string host)
        {
            try
            {
                var uri = new UriBuilder(host).Uri;
                host = uri.Host;

                var entry = await Dns.GetHostEntryAsync(host);
                
                var info = new DnsInfo
                {
                    HostName = entry.HostName,
                    IpAddresses = entry.AddressList.Select(ip => ip.ToString()).ToList()
                };

                return info;
            }
            catch
            {
                return null;
            }
        }

        public string GetAddressType(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
                return "Loopback";
            
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] bytes = address.GetAddressBytes();
                if (bytes[0] == 10 || 
                    (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                    (bytes[0] == 192 && bytes[1] == 168))
                    return "Локальный";
                
                return "Публичный";
            }
            
            return "Неизвестно";
        }
    }
}