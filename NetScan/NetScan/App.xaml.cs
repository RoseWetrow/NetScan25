using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetScan
{
    /// <summary>
    /// Класс приложения инициализирует главный интерфейс — оболочку AppShell.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // обработка запуска при необходимости
        }

        protected override void OnSleep()
        {
            // обработка перехода в фоновый режим
        }

        protected override void OnResume()
        {
            // обработка возврата из фонового режима
        }
    }
}