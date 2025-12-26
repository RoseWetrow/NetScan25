using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetScan
{
    /// Корневая оболочка приложения, определяющая вкладки для сканирования и истории.
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }
    }
}