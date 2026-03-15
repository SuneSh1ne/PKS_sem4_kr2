using System;

namespace PKS_sem4_kr2.Models
{
    public class UrlHistoryItem
    {
        public string Url { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public string Host { get; set; }
        public string Scheme { get; set; }
        public int? Port { get; set; }
        public bool IsValid { get; set; }
    }
}