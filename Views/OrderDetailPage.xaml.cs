namespace CantinaV1.Views;

public partial class OrderDetailPage : ContentPage
{
    public OrderDetailPage(string clientName)
    {
        InitializeComponent();

        // Exibir o nome do cliente na página de detalhes
        ClientNameLabel.Text = clientName;
    }
}