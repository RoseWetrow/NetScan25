using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetScan.ViewModels
{
    /// Базовая ViewModel с реализацией INotifyPropertyChanged (ScanViewModel и HistoryViewModel наследуют её)
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBusy
        {
            get => isBusy;
            set { isBusy = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
