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
        public ObservableCollection<Models.Device> Devices { get; } = new ObservableCollection<Models.Device>();                                                                                                      // при изменении (Add/Clear) поднимает событие CollectionChanged, на которое CollectionView подписан через Binding Devices и обновляет UI (реализует INotifyCollectionChanged)
        public ICommand ScanCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand StopCommand { get; }

        private string subnetText;
        public string SubnetText
        {
            get => subnetText;
            set { subnetText = value; OnPropertyChanged(); }
        }

        public bool ShowSave => !IsBusy;                      /// Показывать кнопку "Сохранить", когда не в состоянии IsBusy
        public bool EnabledSave => Devices.Count > 0;         /// Отображать активной, когда есть элементы и неактивной, когда их нет (изначально)

        public ScanViewModel(INetworkScanner scannerService, IDatabaseService databaseService)
        {
            scanner = scannerService;
            db = databaseService;

            ScanCommand = new Command(async () => await StartScanAsync());
            SaveCommand = new Command(Save);
            StopCommand = new Command(Cancel);
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
            IsBusy = true;                                // показать Стоп
            OnPropertyChanged(nameof(ShowSave));          // скрыть Сохранить
            OnPropertyChanged(nameof(EnabledSave));       // Сохранить неактивна
            cts = new CancellationTokenSource();

            var subnet = GetLocalSubnet();                // определяем подсеть
            if (string.IsNullOrEmpty(subnet))
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowSave));
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось определить локальную подсеть.\nПроверьте подключение и повторите попытку.", "OK");
                return;
            }
            SubnetText = $"Сканируемая сеть: {subnet}";
                // создаётся Progress<Device> с коллбеком, добавляющим найденное устройство device в список устройств Devices (подписка на делегата прогресса)
            var progress = new Progress<Models.Device>(device => {Devices.Add(device);});         
            try
            {   // передаем подсеть в сервис по сканированию на наличие устройств, при обнаружении вызывается progress.Report(device)
                await scanner.ScanSubnetAsync(subnet, progress, cts.Token, maxConcurrency: 30);   
            }
            catch (OperationCanceledException) { /* отмена */ }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowSave));      // кнопки возвращаются в исходное состояние (показываем Сохранить, убираем Стоп)
                OnPropertyChanged(nameof(EnabledSave));
            }
        }

        private void Save()
        {
            if (!Devices.Any()) return;

            db.AddDevices(Devices.Select(d => new Models.Device{Hostname = d.Hostname, Ip = d.Ip, ScanTime = DateTime.Now}));

            MessagingCenter.Send(this, "DevicesSaved");  // Уведомляем, что устройство(я) сохранены в БД. Подписчики (HistoryViewModel) обновят свой список.

            Devices.Clear();                             // Очищаем список устройств после сохранения, кнопка Сохранить становится неактивной
            OnPropertyChanged(nameof(EnabledSave));
        }

        private void Cancel()
        {
            cts?.Cancel();                               // IsBusy будет снят в finally
        }
    }
}

