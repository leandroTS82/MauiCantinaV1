using CantinaV1.Models;
using CantinaV1.Views;

namespace CantinaV1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Força o tema claro
            UserAppTheme = AppTheme.Light;
            MainPage = new MainPage();
        }
    }
}
