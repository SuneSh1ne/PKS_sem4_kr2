using System;
using System.Collections.Generic;
using System.Linq;

namespace PKS_sem4_kr2.Services
{
    public class UrlAnalyzerService
    {
        public class UrlComponents
        {
            public string Scheme { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string Path { get; set; }
            public string Query { get; set; }
            public string Fragment { get; set; }
            public Dictionary<string, string> QueryParameters { get; set; }
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
        }

        public UrlComponents ParseUrl(string urlString)
        {
            var components = new UrlComponents
            {
                QueryParameters = new Dictionary<string, string>()
            };

            try
            {
                if (!urlString.Contains("://"))
                {
                    urlString = "http://" + urlString;
                }

                var uri = new Uri(urlString);
                
                components.Scheme = uri.Scheme;
                components.Host = uri.Host;
                components.Port = uri.Port;
                components.Path = uri.AbsolutePath;
                components.Query = uri.Query;
                components.Fragment = uri.Fragment;
                components.IsValid = true;

                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var query = uri.Query.TrimStart('?');
                    var parameters = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var param in parameters)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2)
                        {
                            components.QueryParameters[parts[0]] = parts[1];
                        }
                    }
                }
            }
            catch (UriFormatException ex)
            {
                components.IsValid = false;
                components.ErrorMessage = $"Ошибка парсинга URL: {ex.Message}";
            }
            catch (Exception ex)
            {
                components.IsValid = false;
                components.ErrorMessage = $"Ошибка: {ex.Message}";
            }

            return components;
        }
    }
}