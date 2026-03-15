using System.Collections.Generic;

namespace PKS_sem4_kr2.Models
{
    public class DnsInfo
    {
        public string HostName { get; set; }
        public List<string> IpAddresses { get; set; } = new List<string>();
        public List<string> Aliases { get; set; } = new List<string>();
    }
}