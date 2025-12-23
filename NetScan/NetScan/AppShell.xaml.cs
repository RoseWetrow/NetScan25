using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetScan
{
    /// <summary>
    /// Корневая оболочка приложения, определяющая вкладки для сканирования и истории.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }
    }
}