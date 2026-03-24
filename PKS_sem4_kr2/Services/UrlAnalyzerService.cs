using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;

namespace PKS_sem4_kr2.Services
{
    public class UrlAnalyzerService
    {
        public class UrlComponents
        {
            public string Scheme { get; set; } = "";
            public string Host { get; set; } = "";
            public int Port { get; set; }
            public string Path { get; set; } = "";
            public string Query { get; set; } = "";
            public string Fragment { get; set; } = "";
            public ObservableCollection<QueryParameter> QueryParameters { get; set; } = new ObservableCollection<QueryParameter>();
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = "";
        }

        public class QueryParameter
        {
            public string Key { get; set; } = "";
            public string Value { get; set; } = "";
        }

        public UrlComponents ParseUrl(string urlString)
        {
            var components = new UrlComponents();

            try
            {
                if (string.IsNullOrWhiteSpace(urlString))
                {
                    components.IsValid = false;
                    components.ErrorMessage = "URL не может быть пустым";
                    return components;
                }

                string urlToParse = urlString;
                if (!urlString.Contains("://") && !urlString.StartsWith("http://") && !urlString.StartsWith("https://"))
                {
                    urlToParse = "http://" + urlString;
                }

                var uri = new Uri(urlToParse);
                
                components.Scheme = uri.Scheme;
                components.Host = uri.Host;
                components.Port = uri.Port;
                components.Path = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;
                components.Query = uri.Query;
                components.Fragment = uri.Fragment;
                components.IsValid = true;

                if (!string.IsNullOrEmpty(uri.Query))
                {
                    try
                    {
                        var query = uri.Query.TrimStart('?');
                        var parameters = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
                        
                        components.QueryParameters.Clear();
                        foreach (var param in parameters)
                        {
                            var parts = param.Split('=');
                            if (parts.Length > 0)
                            {
                                var key = System.Net.WebUtility.UrlDecode(parts[0]);
                                var value = parts.Length > 1 ? System.Net.WebUtility.UrlDecode(parts[1]) : "";
                                
                                components.QueryParameters.Add(new QueryParameter
                                {
                                    Key = key,
                                    Value = value
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка парсинга параметров: {ex.Message}");
                    }
                }
            }
            catch (UriFormatException ex)
            {
                components.IsValid = false;
                components.ErrorMessage = $"Ошибка формата URL: {ex.Message}";
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