using CantinaV1.Services.Internals;

namespace CantinaV1.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void GoToProductsPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProductsPage());
        }

        private async void GoToOrderPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrderPage());
        }
        private async void GoToConfigurationPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConfigurationPage());
        }
        private async void GoToAdvancedSettingsPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdvancedSettingsPage());
        }

        private async void OnRedirectReceivedOrdersPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReceivedOrdersPage());
        }        

        private async void GoToOrderHistoryPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrderHistoryPage());
        }
    }
}
