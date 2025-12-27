using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NetScan.Models;
using NetScan.Services;
using Xamarin.Forms;

namespace NetScan.ViewModels
{
    /// ViewModel для страницы истории. Загружает данные из БД и предоставляет команду очистки.
    public class HistoryViewModel : BaseViewModel
    {
        private readonly IDatabaseService db;
        public ObservableCollection<Models.Device> History { get; } = new ObservableCollection<Models.Device>();

        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }
        public bool EmptyHistory => History.Count == 0;
        public bool FilledHistory => History.Count > 0;

        public HistoryViewModel(IDatabaseService databaseService)
        {
            db = databaseService;
            RefreshCommand = new Command(Load);
            ClearCommand = new Command(Clear);
            Load();

            // Подписываемся на событие сохранения устройств — обновляем историю сразу.
            MessagingCenter.Subscribe<ScanViewModel>(this, "DevicesSaved", (sender) => {Load();});
        }

        /// Запрашивает список устройств из базы данных (сортировка по убыванию Id)
        private void Load()
        {
            History.Clear();
            var list = db.GetDevices();
            foreach (var d in list)
                History.Add(d);

            OnPropertyChanged(nameof(FilledHistory));  /// показываем Очистить
            OnPropertyChanged(nameof(EmptyHistory));   /// информационная надпись пропадает
        }

        /// Обработчик кнопки Очистить, вызывает метод для удаления всех записей и обновляет список.
        private void Clear()
        {
            db.DeleteAllDevices();
            History.Clear();
            OnPropertyChanged(nameof(EmptyHistory));   /// возвращаем информационную надпись и убираем Очистить
            OnPropertyChanged(nameof(FilledHistory));
        }
    }
}
