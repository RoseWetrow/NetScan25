using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NetScan.Models;
using NetScan.Services;
using Xamarin.Forms;

namespace NetScan.ViewModels
{
    /// ViewModel для страницы сканирования
    /// Перенесена и адаптирована логика из ScanPage: старт/остановка сканирования, накопление обнаруженных устройств, сохранение в БД
    public class ScanViewModel : BaseViewModel
    {
        private readonly INetworkScanner scanner;
        private readonly IDatabaseService db;
        private CancellationTokenSource cts;

        // Список обнаруженных устройств до момента сохранения
        public ObservableCollection<Models.Device> Devices { get; } = new ObservableCollection<Models.Device>();

        public ICommand ScanCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private string subnetText;
        public string SubnetText
        {
            get => subnetText;
            set { subnetText = value; OnPropertyChanged(); }
        }

        /// Показывать кнопку "Сохранить": когда не в состоянии IsBusy и есть элементы
        public bool ShowSave => !IsBusy && Devices.Count > 0;

        public ScanViewModel(INetworkScanner scannerService, IDatabaseService databaseService)
        {
            scanner = scannerService;
            db = databaseService;

            ScanCommand = new Command(async () => await StartScanAsync());
            SaveCommand = new Command(Save);
            CancelCommand = new Command(Cancel);
        }

        private string GetLocalSubnet()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in interfaces)
            {
                var properties = adapter.GetIPProperties();
                foreach (var unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        var bytes = unicast.Address.GetAddressBytes();
                        var subnet = $"{bytes[0]}.{bytes[1]}.{bytes[2]}";
                        if (subnet != "127.0.0")
                            return subnet;
                    }
                }
            }
            return null;
        }

        public async Task StartScanAsync()
        {
            Devices.Clear();
            OnPropertyChanged(nameof(ShowSave)); // скрыть Save сразу
            IsBusy = true;
            cts = new CancellationTokenSource();

            var subnet = GetLocalSubnet();
            if (string.IsNullOrEmpty(subnet))
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowSave));
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось определить локальную подсеть", "OK");
                return;
            }

            SubnetText = $"Сканируемая сеть: {subnet}";

            var progress = new Progress<Models.Device>(device =>
            {
                Devices.Add(device);
                OnPropertyChanged(nameof(ShowSave));  // После добавления устройства обновляем ShowSave (хотя пока IsBusy=true)
            });

            try
            {
                await scanner.ScanSubnetAsync(subnet, progress, cts.Token, maxConcurrency: 40);
            }
            catch (OperationCanceledException) { /* отмена */ }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowSave));  // После окончания сканирования ShowSave может измениться
            }
        }

        private void Save()
        {
            if (!Devices.Any()) return;

            db.AddDevices(Devices.Select(d => new Models.Device{Hostname = d.Hostname, Ip = d.Ip, ScanTime = DateTime.Now}));

            //Devices.Clear();
            OnPropertyChanged(nameof(ShowSave)); // скрыть Save после сохранения

            // Уведомляем, что устройство(я) сохранены в БД. Подписчики (HistoryViewModel) обновят свой список.
            MessagingCenter.Send(this, "DevicesSaved");
        }

        private void Cancel()
        {
            cts?.Cancel();  // IsBusy будет снят в finally блока StartScanAsync, там мы вызовем OnPropertyChanged(ShowSave)
        }
    }
}

