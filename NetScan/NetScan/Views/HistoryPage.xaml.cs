using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NetScan.Services;
using NetScan.ViewModels;


namespace NetScan.Views
{
    /// Страница истории выводит данные, сохранённые в SQLite-базе, и
    /// предоставляет возможность очистить историю. История обновляется
    /// автоматически при каждом открытии страницы.
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage()
        {
            InitializeComponent();

            var db = new DatabaseService();
            BindingContext = new HistoryViewModel(db);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // ViewModel сам загружает историю в конструкторе/RefreshCommand.
            // Если нужно — можно явно вызвать привязанный RefreshCommand:
            // ((HistoryViewModel)BindingContext)?.RefreshCommand?.Execute(null);
        }
    }
}

