using System.Diagnostics;

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
        private void OnSendMessageClicked(object sender, EventArgs e)
        {
            string phoneNumber = "5511960842249";
            string message = "Olá, esta é uma mensagem de teste!";
            string url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(message)}";

            try
            {
                Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao abrir o WhatsApp: {ex.Message}");
            }
        }
    }
}
