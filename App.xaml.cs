using CantinaV1.FilesConfiguration;
using CantinaV1.Views;
using System.Threading.Tasks;

namespace CantinaV1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Força o tema claro
            UserAppTheme = AppTheme.Light;

            InitializeAppAsync(); // Chamada segura
            

            MainPage = new NavigationPage(new MainPage());
        }

        private async Task InitializeAppAsync()
        {
            AppInitializer appInitializer = new AppInitializer();
            await appInitializer.InitializeDefaultsAsync();
        }
    }
}
