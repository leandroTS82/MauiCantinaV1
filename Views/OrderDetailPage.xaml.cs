namespace CantinaV1.Views;

public partial class OrderDetailPage : ContentPage
{
    public OrderDetailPage(string clientName)
    {
        InitializeComponent();

        // Exibir o nome do cliente na p�gina de detalhes
        ClientNameLabel.Text = clientName;
    }
}