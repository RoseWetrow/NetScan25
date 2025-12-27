using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NetScan.Services;
using NetScan.ViewModels;


namespace NetScan.Views
{
    /// Страница сканирования локальной сети
    /// (Комментарии из оригинала сохранены в ViewModel/Services)
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanPage : ContentPage
    {
        public ScanPage()
        {
            InitializeComponent();

            // Создаём сервисы и ViewModel (простая инъекция зависимостей)
            var scanner = new NetworkScannerService();
            var db = new DatabaseService();
            BindingContext = new ScanViewModel(scanner, db);

            // Вся логика сканирования и работы с БД в ViewModel/Services
            // В code-behind остаётся только инициализация и минимальные UI-взаимодействия
        }
    }
}
