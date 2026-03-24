using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PKS_sem4_kr2.Models;
using PKS_sem4_kr2.Services;

namespace PKS_sem4_kr2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NetworkService _networkService;
        private readonly UrlAnalyzerService _urlAnalyzerService;
        
        private NetworkInterfaceInfo _selectedNetworkInterface;
        private string _inputUrl;
        private UrlAnalyzerService.UrlComponents _urlComponents;
        private DnsInfo _dnsInfo;
        private string _pingResult;
        private string _addressType;

        public ObservableCollection<NetworkInterfaceInfo> NetworkInterfaces { get; set; }
        public ObservableCollection<UrlHistoryItem> UrlHistory { get; set; }

        public ICommand AnalyzeUrlCommand { get; }
        public ICommand PingHostCommand { get; }
        public ICommand GetDnsInfoCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public MainViewModel()
        {
            _networkService = new NetworkService();
            _urlAnalyzerService = new UrlAnalyzerService();
            
            NetworkInterfaces = new ObservableCollection<NetworkInterfaceInfo>();
            UrlHistory = new ObservableCollection<UrlHistoryItem>();
            
            LoadNetworkInterfaces();
            
            AnalyzeUrlCommand = new RelayCommand(AnalyzeUrl);
            PingHostCommand = new RelayCommand(async () => await PingHostAsync(), () => UrlComponents?.IsValid == true);
            GetDnsInfoCommand = new RelayCommand(async () => await GetDnsInfoAsync(), () => UrlComponents?.IsValid == true);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
        }

        public NetworkInterfaceInfo SelectedNetworkInterface
        {
            get => _selectedNetworkInterface;
            set
            {
                _selectedNetworkInterface = value;
                OnPropertyChanged();
            }
        }

        public string InputUrl
        {
            get => _inputUrl;
            set
            {
                _inputUrl = value;
                OnPropertyChanged();
            }
        }

        public UrlAnalyzerService.UrlComponents UrlComponents
        {
            get => _urlComponents;
            set
            {
                _urlComponents = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasUrlComponents));
                (PingHostCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (GetDnsInfoCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool HasUrlComponents => UrlComponents != null && UrlComponents.IsValid;

        public DnsInfo DnsInfo
        {
            get => _dnsInfo;
            set
            {
                _dnsInfo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDnsInfo));
            }
        }

        public bool HasDnsInfo => DnsInfo != null;

        public string PingResult
        {
            get => _pingResult;
            set
            {
                _pingResult = value;
                OnPropertyChanged();
            }
        }

        public string AddressType
        {
            get => _addressType;
            set
            {
                _addressType = value;
                OnPropertyChanged();
            }
        }

        private void LoadNetworkInterfaces()
        {
            try
            {
                var interfaces = _networkService.GetNetworkInterfaces();
                NetworkInterfaces.Clear();
                foreach (var ni in interfaces)
                {
                    NetworkInterfaces.Add(ni);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сетевых интерфейсов: {ex.Message}");
            }
        }

        private void AnalyzeUrl()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InputUrl))
                {
                    MessageBox.Show("Введите URL для анализа", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DnsInfo = null;
                PingResult = "";
                AddressType = "";

                UrlComponents = _urlAnalyzerService.ParseUrl(InputUrl);
                
                if (UrlComponents.IsValid)
                {
                    try
                    {
                        UrlHistory.Add(new UrlHistoryItem
                        {
                            Url = InputUrl,
                            AnalyzedAt = DateTime.Now,
                            Host = UrlComponents.Host,
                            Scheme = UrlComponents.Scheme,
                            Port = UrlComponents.Port,
                            IsValid = true
                        });

                        while (UrlHistory.Count > 50)
                        {
                            UrlHistory.RemoveAt(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка добавления в историю: {ex.Message}");
                    }

                    try
                    {
                        var host = UrlComponents.Host;
                        var addresses = System.Net.Dns.GetHostAddresses(host);
                        if (addresses.Length > 0)
                        {
                            AddressType = _networkService.GetAddressType(addresses[0]);
                        }
                        else
                        {
                            AddressType = "Не удалось определить";
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка определения типа адреса: {ex.Message}");
                        AddressType = "Не удалось определить";
                    }
                }
                else
                {
                    MessageBox.Show(UrlComponents.ErrorMessage, "Ошибка парсинга URL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка анализа URL: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Ошибка AnalyzeUrl: {ex}");
            }
        }

        private async Task PingHostAsync()
        {
            if (UrlComponents == null || !UrlComponents.IsValid || string.IsNullOrEmpty(UrlComponents.Host))
            {
                MessageBox.Show("Сначала проанализируйте корректный URL", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PingResult = "Пингование...";
            try
            {
                var result = await _networkService.PingHostAsync(UrlComponents.Host);
                PingResult = result.Success 
                    ? $"✓ Успешно! Время ответа: {result.RoundtripTime} мс" 
                    : "✗ Хост недоступен";
            }
            catch (Exception ex)
            {
                PingResult = $"✗ Ошибка пингования: {ex.Message}";
            }
        }

        private async Task GetDnsInfoAsync()
        {
            if (UrlComponents == null || !UrlComponents.IsValid || string.IsNullOrEmpty(UrlComponents.Host))
            {
                MessageBox.Show("Сначала проанализируйте корректный URL", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DnsInfo = null;
            try
            {
                DnsInfo = await _networkService.GetDnsInfoAsync(InputUrl);
                if (DnsInfo == null)
                {
                    MessageBox.Show("Не удалось получить DNS информацию для указанного хоста", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения DNS информации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearHistory()
        {
            UrlHistory.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}