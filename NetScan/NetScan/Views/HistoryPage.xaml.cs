using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static NetScan.Views.ScanPage;

namespace NetScan.Views
{
    /// <summary>
    /// Страница истории выводит данные, сохранённые в SQLite‑базе, и
    /// предоставляет возможность очистить историю. История обновляется
    /// автоматически при каждом открытии страницы.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// При появлении страницы перечитываем список устройств из базы и
        /// обновляем источник данных ListView.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateHistoryList();
        }

        /// <summary>
        /// Запрашивает список устройств из базы данных. Порядок сортировки –
        /// по убыванию идентификатора, чтобы новые записи отображались первыми.
        /// </summary>
        private List<ScanPage.Device> GetDevices()
        {
            using (SQLiteConnection connection = GetConnection())
            {
                var devices = connection
                    .Query<ScanPage.Device>("SELECT * FROM Device ORDER BY Id DESC")
                    .ToList();
                return devices;
            }
        }

        /// <summary>
        /// Обновляет источник данных ListView. Сначала сбрасывает ItemsSource,
        /// затем назначает новую коллекцию, чтобы визуальный компонент
        /// корректно перерисовался.
        /// </summary>
        public void UpdateHistoryList()
        {
            listView.ItemsSource = null;
            listView.ItemsSource = GetDevices();
        }

        /// <summary>
        /// Обработчик кнопки «Очистить». Вызывает статический метод для
        /// удаления всех записей и заново обновляет список.
        /// </summary>
        private void Button_Clear(object sender, EventArgs e)
        {
            Console.WriteLine("Сработал обработчик Button_Clear !");
            DeleteAllHistory();
            UpdateHistoryList();
        }
    }
}