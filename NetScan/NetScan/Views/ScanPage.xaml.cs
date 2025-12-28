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
        }
    }
}