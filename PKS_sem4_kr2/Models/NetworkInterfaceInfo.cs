using System;
using System.Net.NetworkInformation;

namespace PKS_sem4_kr2.Models
{
    public class NetworkInterfaceInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public string MacAddress { get; set; }
        public OperationalStatus Status { get; set; }
        public long Speed { get; set; }
        public string SpeedFormatted => Speed > 0 ? $"{Speed / 1000000} Мбит/с" : "Недоступно";
        public NetworkInterfaceType InterfaceType { get; set; }
        public bool IsOperational => Status == OperationalStatus.Up;
    }
}